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

[Generator(LanguageNames.CSharp)]
public class Generator : IIncrementalGenerator
{
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
        List<TypeToGenerate> endpointsToGenerate = GetTypesToGenerate(compilation, typeDeclarations.Distinct(), context);

        foreach (TypeToGenerate typeToGenerate in endpointsToGenerate)
        {
            string source = SourceGenerator.Generate(typeToGenerate);
            context.AddSource(typeToGenerate.HintName, source);
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

    private static List<TypeToGenerate> GetTypesToGenerate(Compilation compilation, IEnumerable<TypeDeclarationSyntax> types, SourceProductionContext context)
    {
        System.Threading.CancellationToken ct = context.CancellationToken;
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
                ReportDiagnostic(context, "ST0003", "Types must be defined partial", location);
                continue;
            }

            if (symbol.IsFileLocal)
            {
                ReportDiagnostic(context, "ST0009", "File-local strong types are not supported; remove the file modifier", location);
                continue;
            }

            if (symbol.IsStatic)
            {
                ReportDiagnostic(context, "ST0011", "Static strong types are not supported; remove the static modifier", location);
                continue;
            }
            if (symbol.IsAbstract)
            {
                ReportDiagnostic(context, "ST0012", "Abstract strong types are not supported; remove the abstract modifier", location);
                continue;
            }
            if (symbol.IsRefLikeType)
            {
                ReportDiagnostic(context, "ST0013", "Ref struct strong types are not supported; remove the ref modifier", location);
                continue;
            }

            if (symbol.DeclaringSyntaxReferences.Any(reference =>
                reference.GetSyntax(ct) is TypeDeclarationSyntax type && type.ParameterList != null))
            {
                ReportDiagnostic(context, "ST0010", "Primary constructors are not supported for strong types; remove the parameter list", location);
                continue;
            }

            if (symbol.ContainingNamespace.IsGlobalNamespace)
            {
                ReportDiagnostic(context, "ST0005", "Strong types must be declared in a namespace", location);
                continue;
            }
            if (symbol.ContainingType != null)
            {
                ReportDiagnostic(context, "ST0006", "Nested strong types are not supported; declare the type at namespace scope", location);
                continue;
            }
            if (symbol.Arity > 0)
            {
                ReportDiagnostic(context, "ST0007", "Generic strong types are not supported", location);
                continue;
            }

            object baseType = attribute.ConstructorArguments[0].Value;
            if ((baseType is 3 || baseType is 4) && declaration is not ClassDeclarationSyntax)
            {
                ReportDiagnostic(context, "ST0008", "String and email strong types must be declared as classes, not structs or records", location);
                continue;
            }

            TypeToGenerate typeToGenerate = new()
            {
                DeclarationSyntax = declaration,
                Namespace = compilation.Namespace(declaration),
                HintName = GetHintName(symbol),
                ValueFieldName = GetAvailableMemberName(symbol, "_value"),
                ConverterName = GetAvailableMemberName(symbol, "JsonConverter"),
            };

            // Roslyn resolves aliases, constant expressions, and named argument order.
            switch (attribute.ConstructorArguments[0].Value)
            {
                case 0: // BaseType.Integer
                    typeToGenerate.BaseTypeName = "int";
                    break;
                case 1: // BaseType.Guid
                    typeToGenerate.BaseTypeName = "global::System.Guid";
                    break;
                case 2: // BaseType.Uri
                    if (declaration is not ClassDeclarationSyntax)
                    {
                        ReportDiagnostic(context, "ST0002", "Only class supported for Uri type", location);
                        continue;
                    }
                    typeToGenerate.BaseTypeName = "global::System.Uri";
                    break;
                case 3: // BaseType.String
                    typeToGenerate.BaseTypeName = "string";
                    if (attribute.ConstructorArguments.Length > 1)
                    {
                        typeToGenerate.MaxLength = (int)attribute.ConstructorArguments[1].Value;
                        if (typeToGenerate.MaxLength < 0)
                        {
                            ReportDiagnostic(context, "ST0004", "Maximum length must be non-negative", location);
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
                    ReportDiagnostic(context, "ST0001", "Base type is not supported", location);
                    continue;
            }
            if (ConflictsWithPublicMemberName(symbol.Name, typeToGenerate))
            {
                ReportDiagnostic(context, "ST0014",
                    $"Strong type name '{symbol.Name}' conflicts with a generated public member; rename the type", location);
                continue;
            }

            typesToGenerate.Add(typeToGenerate);
        }
        return typesToGenerate;
    }

    private static bool ConflictsWithPublicMemberName(string name, TypeToGenerate type)
    {
        if (name is "Value" or "ToString" or "Equals" or "GetHashCode")
        {
            return true;
        }
        if (type.BaseTypeName is "int" or "global::System.Guid"
            && name is "Parse" or "TryParse" or "CompareTo")
        {
            return true;
        }
        return type.BaseTypeName == "global::System.Guid" && name is "New" or "Empty";
    }

    private static string GetAvailableMemberName(INamedTypeSymbol symbol, string preferredName)
    {
        string name = preferredName;
        int suffix = 0;
        while (name == symbol.Name || symbol.GetMembers(name).Length > 0)
        {
            name = preferredName + (++suffix).ToString(System.Globalization.CultureInfo.InvariantCulture);
        }
        return name;
    }

    private static string GetHintName(INamedTypeSymbol symbol)
    {
        // Metadata names preserve generic arity; '+' distinguishes containing types
        // from namespaces without introducing filesystem path separators.
        Stack<string> typeNames = new();
        for (INamedTypeSymbol type = symbol; type != null; type = type.ContainingType)
        {
            typeNames.Push(type.MetadataName);
        }

        string prefix = symbol.ContainingNamespace.IsGlobalNamespace
            ? string.Empty : symbol.ContainingNamespace.ToDisplayString() + ".";
        return prefix + string.Join("+", typeNames) + ".g.cs";
    }

    private static void ReportDiagnostic(SourceProductionContext context, string id, string message, Location location)
    {
        context.ReportDiagnostic(Diagnostic.Create(
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
