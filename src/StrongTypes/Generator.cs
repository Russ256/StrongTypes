namespace StrongTypes;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using StrongTypes.SourceGenerators;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading;

[Generator(LanguageNames.CSharp)]
public class Generator : IIncrementalGenerator
{
    private static SourceProductionContext _sourceProductionContext;
    internal const string _namespace = "StrongTypes";
    internal const string _attributeName = "StrongType";

    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        // Add our attribute to the project
        context.RegisterPostInitializationOutput(ctx => ctx.AddSource(
                                                        "StrongTypes.Attributes.g.cs",
                                                        SourceText.From(Attributes.Source, Encoding.UTF8)));

        // Add our exceptions to the project
        context.RegisterPostInitializationOutput(ctx => ctx.AddSource(
                                                        "StrongTypes.Exceptions.g.cs",
                                                        SourceText.From(Exceptions.Source, Encoding.UTF8)));

        // Create a simple filter for class
        IncrementalValuesProvider<TypeDeclarationSyntax> typesProvider = context.SyntaxProvider
            .CreateSyntaxProvider(
                predicate: static (s, _) => IsSyntaxTargetForGeneration(s), // select only classes with attributes
                transform: static (ctx, _) => GetSemanticTargetForGeneration(ctx)) // select types with our attribute
            .Where(static m => m is not null)!; // filter out attributed classes that we don't care about

        IncrementalValueProvider<(Compilation, ImmutableArray<TypeDeclarationSyntax>)> valueProviders
          = context.CompilationProvider.Combine(typesProvider.Collect());

        context.RegisterSourceOutput(valueProviders,
            static (spc, source) => Execute(source.Item1, source.Item2, spc));
    }

    private static void Execute(Compilation compilation, ImmutableArray<TypeDeclarationSyntax> typeDeclarations, SourceProductionContext context)
    {
        _sourceProductionContext = context;

        List<TypeToGenerate> endpointsToGenerate = GetTypesToGenerate(compilation, typeDeclarations.Distinct(), context.CancellationToken);

        foreach (TypeToGenerate typeToGenerate in endpointsToGenerate)
        {
            string source = SourceGenerator.Generate(typeToGenerate);
            context.AddSource($"{typeToGenerate.Name}.g.cs", source);
        }
    }

    private static TypeDeclarationSyntax GetSemanticTargetForGeneration(GeneratorSyntaxContext context)
    {
        TypeDeclarationSyntax declaration = (TypeDeclarationSyntax)context.Node;
        INamedTypeSymbol symbol = context.SemanticModel.GetDeclaredSymbol(declaration) as INamedTypeSymbol;
        return symbol?.GetAttributes().Any(attribute => IsStrongTypeAttribute(attribute)
            && attribute.ApplicationSyntaxReference?.SyntaxTree == declaration.SyntaxTree
            && declaration.Span.Contains(attribute.ApplicationSyntaxReference.Span)) == true
            ? declaration : null;
    }

    private static bool IsStrongTypeAttribute(AttributeData attribute)
        => attribute.AttributeClass?.ToDisplayString() == "StrongTypes.StrongTypeAttribute";

    private static List<TypeToGenerate> GetTypesToGenerate(Compilation compilation, IEnumerable<TypeDeclarationSyntax> types, CancellationToken ct)
    {
        List<TypeToGenerate> typesToGenerate = [];
        foreach (TypeDeclarationSyntax declaration in types)
        {
            ct.ThrowIfCancellationRequested();
            INamedTypeSymbol symbol = compilation.GetSemanticModel(declaration.SyntaxTree)
                .GetDeclaredSymbol(declaration, ct) as INamedTypeSymbol;
            AttributeData attribute = symbol?.GetAttributes().FirstOrDefault(IsStrongTypeAttribute);
            if (attribute == null || attribute.ConstructorArguments.Length == 0
                || attribute.ConstructorArguments.Any(argument => argument.Kind == TypedConstantKind.Error))
            {
                // Invalid expressions already receive compiler diagnostics.
                continue;
            }

            Location location = attribute.ApplicationSyntaxReference?.GetSyntax(ct).GetLocation()
                ?? declaration.GetLocation();
            if (!declaration.Modifiers.Any(modifier => modifier.Text == "partial"))
            {
                ReportDiagnostic("ST0003", "Types must be defined partial", location);
                continue;
            }

            TypeToGenerate typeToGenerate = new()
            {
                DeclarationSyntax = declaration,
                Namespace = compilation.Namespace(declaration),
            };

            // Roslyn resolves aliases, constant expressions, and named argument order.
            switch (attribute.ConstructorArguments[0].Value)
            {
                case 0: // BaseType.Integer
                    typeToGenerate.BaseTypeName = "int";
                    break;
                case 1: // BaseType.Guid
                    typeToGenerate.BaseTypeName = "System.Guid";
                    break;
                case 2: // BaseType.Uri
                    if (declaration is not ClassDeclarationSyntax)
                    {
                        ReportDiagnostic("ST0002", "Only class supported for Uri type", location);
                        continue;
                    }
                    typeToGenerate.BaseTypeName = "System.Uri";
                    break;
                case 3: // BaseType.String
                    typeToGenerate.BaseTypeName = "string";
                    if (attribute.ConstructorArguments.Length > 1)
                    {
                        typeToGenerate.MaxLength = (int)attribute.ConstructorArguments[1].Value;
                        if (typeToGenerate.MaxLength < 0)
                        {
                            ReportDiagnostic("ST0004", "Maximum length must be non-negative", location);
                            continue;
                        }
                    }
                    break;
                case 4: // BaseType.EmailAddress
                    typeToGenerate.BaseTypeName = "string";
                    typeToGenerate.IsEmail = true;
                    typeToGenerate.MaxLength = 254;
                    break;
                default:
                    ReportDiagnostic("ST0001", "Base type is not supported", location);
                    continue;
            }
            typesToGenerate.Add(typeToGenerate);
        }
        return typesToGenerate;
    }

    private static void ReportDiagnostic(string id, string message, Location location)
    {
        _sourceProductionContext.ReportDiagnostic(Diagnostic.Create(
            new DiagnosticDescriptor(id, "Not Supported", message, "Strong Types", DiagnosticSeverity.Error, true),
            location));
    }
    private static bool IsSyntaxTargetForGeneration(SyntaxNode node)
    {
        return node is ClassDeclarationSyntax m && m.AttributeLists.Count > 0
            || node is RecordDeclarationSyntax r && r.AttributeLists.Count > 0
            || node is StructDeclarationSyntax s && s.AttributeLists.Count > 0;
    }

    public static AttributeArgumentSyntax GetArgumentByName(AttributeArgumentListSyntax argumentList, string name)
    {
        if (argumentList == null) return null;

        foreach (AttributeArgumentSyntax arg in argumentList.Arguments)
        {
            if (arg.NameEquals != null &&
                string.Equals(arg.NameEquals.Name.Identifier.ValueText, name, StringComparison.OrdinalIgnoreCase))
            {
                return arg;
            }
        }
        return null;
    }
}
