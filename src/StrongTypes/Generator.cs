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
    private static readonly string[] _supportedTypes = ["int", "System.Guid", "System.Uri", "string"];
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
        // we know the node is a TypeDeclarationSyntax thanks to IsSyntaxTargetForGeneration
        TypeDeclarationSyntax typeDeclarationSyntax = (TypeDeclarationSyntax)context.Node;

        // loop through all the attributes on the method
        foreach (AttributeListSyntax attributeListSyntax in typeDeclarationSyntax.AttributeLists)
        {
            foreach (AttributeSyntax attributeSyntax in attributeListSyntax.Attributes)
            {
                ISymbol attributeSymbol = context.SemanticModel.GetSymbolInfo(attributeSyntax).Symbol;

                INamedTypeSymbol attributeContainingTypeSymbol = attributeSymbol.ContainingType;
                string fullName = attributeContainingTypeSymbol.ToDisplayString();

                // Is the attribute the [EnumExtensions] attribute?
                if (fullName == "StrongTypes.StrongTypeAttribute")
                {
                    // Found it!
                    return typeDeclarationSyntax;
                }
            }
        }

        // Nothing found
        return null;
    }

    private static List<TypeToGenerate> GetTypesToGenerate(Compilation compilation, IEnumerable<TypeDeclarationSyntax> types, CancellationToken ct)
    {
        List<TypeToGenerate> typesToGenerate = [];

        foreach (TypeDeclarationSyntax typeDeclaration in types)
        {
            ct.ThrowIfCancellationRequested();

            TypeToGenerate typeToGenerate = new()
            {
                DeclarationSyntax = typeDeclaration,
                Namespace = compilation.Namespace(typeDeclaration),
            };

            // Identify the type to generate
            AttributeSyntax attr = compilation.GetAttribute(typeDeclaration, _attributeName);

            // Get type from attribute
            ExpressionSyntax baseTypeExpression = attr.ArgumentList.Arguments[0].Expression;

            // Partial type check
            if (!typeDeclaration.Modifiers.Any(td => td.Text == "partial"))
            {
                Diagnostic diagnostic = Diagnostic.Create(new DiagnosticDescriptor("ST0003",
                                       "Not Supported",
                                       "Types must be defined partial",
                                       "Strong Types",
                                       DiagnosticSeverity.Error, true),
                                       attr.ArgumentList.Arguments[0].GetLocation());

                _sourceProductionContext.ReportDiagnostic(diagnostic);
                continue;
            }

            // Identify type.
            string baseType = baseTypeExpression.ToFullString();
            if (baseType == "BaseType.Integer")
            {
                typeToGenerate.BaseTypeName = "int";
            }
            else if (baseType == "BaseType.Guid")
            {
                typeToGenerate.BaseTypeName = "System.Guid";
            }
            else if (baseType == "BaseType.Uri")
            {
                if (typeDeclaration is not ClassDeclarationSyntax)
                {
                    Diagnostic diagnostic = Diagnostic.Create(new DiagnosticDescriptor("ST0002",
                                           "Not Supported",
                                           "Only class supported for Uri type",
                                           "Strong Types",
                                           DiagnosticSeverity.Error, true),
                                           attr.ArgumentList.Arguments[0].GetLocation());

                    _sourceProductionContext.ReportDiagnostic(diagnostic);
                    continue;
                }
                else
                {
                    typeToGenerate.BaseTypeName = "System.Uri";
                }
            }
            else if (baseType == "BaseType.String")
            {
                typeToGenerate.BaseTypeName = "string";

                if (attr.ArgumentList.Arguments.Count > 1)
                {
                    ExpressionSyntax mlExpression = attr.ArgumentList.Arguments[1].Expression;
                    typeToGenerate.MaxLength = int.Parse(mlExpression.ToString());
                }
            }


            //SemanticModel sm = compilation.GetSemanticModel(typeDeclaration.SyntaxTree);
            //ITypeSymbol typeSymbol = sm.GetTypeInfo(expression.Type).Type;
            //string type = typeSymbol.ToString();
            //if (type == "System.Int32") type = "int";

            //if (!_supportedTypes.Contains(type))
            //{
            //    Diagnostic diagnostic = Diagnostic.Create(new DiagnosticDescriptor("ST0001",
            //                           "Not Supported",
            //                           "Type not supported",
            //                           "Strong Types",
            //                           DiagnosticSeverity.Error, true),
            //                           attr.ArgumentList.Arguments[0].GetLocation());

            //    _sourceProductionContext.ReportDiagnostic(diagnostic);
            //    continue;
            //}

            //if (type == "System.Uri" && typeDeclaration is not ClassDeclarationSyntax)
            //{
            //    Diagnostic diagnostic = Diagnostic.Create(new DiagnosticDescriptor("ST0002",
            //                           "Not Supported",
            //                           "Only class supported for Uri type",
            //                           "Strong Types",
            //                           DiagnosticSeverity.Error, true),
            //                           attr.ArgumentList.Arguments[0].GetLocation());

            //    _sourceProductionContext.ReportDiagnostic(diagnostic);
            //    continue;
            //}

            //if (!typeDeclaration.Modifiers.Any(td => td.Text == "partial"))
            //{
            //    Diagnostic diagnostic = Diagnostic.Create(new DiagnosticDescriptor("ST0003",
            //                           "Not Supported",
            //                           "Types must be defined partial",
            //                           "Strong Types",
            //                           DiagnosticSeverity.Error, true),
            //                           attr.ArgumentList.Arguments[0].GetLocation());

            //    _sourceProductionContext.ReportDiagnostic(diagnostic);
            //    continue;
            //}

            typesToGenerate.Add(typeToGenerate);
        }

        return typesToGenerate;
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

        foreach (var arg in argumentList.Arguments)
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