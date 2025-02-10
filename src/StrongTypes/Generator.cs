namespace StrongTypes;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading;

[Generator(LanguageNames.CSharp)]
public class Generator : IIncrementalGenerator
{
    private static readonly string[] supportedTypes = ["int", "System.Guid"];
    private static SourceProductionContext sourceProductionContext;

    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        // Add our attribute to the project
        context.RegisterPostInitializationOutput(ctx => ctx.AddSource(
                                                        "StringTypes.Attributes.g.cs",
                                                        SourceText.From(Attributes.Source,
                                                        Encoding.UTF8)));

        // Create a simple filter for class
        IncrementalValuesProvider<TypeDeclarationSyntax> enumDeclarations = context.SyntaxProvider
            .CreateSyntaxProvider(
                predicate: static (s, _) => IsSyntaxTargetForGeneration(s), // select classes with attributes
                transform: static (ctx, _) => GetSemanticTargetForGeneration(ctx)) // sect the enum with the [MinApiGet] attribute
            .Where(static m => m is not null)!; // filter out attributed classes that we don't care about

        IncrementalValueProvider<(Compilation, ImmutableArray<TypeDeclarationSyntax>)> compilationAndClasses
          = context.CompilationProvider.Combine(enumDeclarations.Collect());

        // Generate the source using the compilation and enums
        context.RegisterSourceOutput(compilationAndClasses,
            static (spc, source) => Execute(source.Item1, source.Item2, spc));
    }

    private static bool IsSyntaxTargetForGeneration(SyntaxNode node)
    {
        return node is TypeDeclarationSyntax m && m.AttributeLists.Count > 0;
    }

    private static List<TypeToGenerate> GetTypesToGenerate(Compilation compilation, IEnumerable<TypeDeclarationSyntax> types, CancellationToken ct)
    {
        List<TypeToGenerate> typesToGenerate = [];

        foreach (TypeDeclarationSyntax typeDeclaration in types)
        {
            ct.ThrowIfCancellationRequested();

            // Identify the type to generate
            AttributeSyntax attr = GetAttribute(typeDeclaration);
            TypeOfExpressionSyntax expression = attr.ArgumentList.Arguments[0].Expression as TypeOfExpressionSyntax;
            SemanticModel sm = compilation.GetSemanticModel(typeDeclaration.SyntaxTree);
            ITypeSymbol typeSymbol = sm.GetTypeInfo(expression.Type).Type;
            string type = typeSymbol.ToString();
            if (type == "System.Int32") type = "int";            

            if (!supportedTypes.Contains(type))
            {
                Diagnostic diagnostic = Diagnostic.Create(new DiagnosticDescriptor("ST0001",
                                       "Not Supported",
                                       "Type not supported",
                                       "Strong Types",
                                       DiagnosticSeverity.Error, true), 
                                       attr.ArgumentList.Arguments[0].GetLocation());

                sourceProductionContext.ReportDiagnostic(diagnostic);
                continue;
            }

            typesToGenerate.Add(new TypeToGenerate()
            {
                ClassStruct = typeDeclaration.Keyword.Text,
                Name = typeDeclaration.Identifier.Text,
                Namespace = GetNamespace(compilation, typeDeclaration),
                BaseType = type,
                Modifiers = typeDeclaration.Modifiers.ToString()
            });
        }

        return typesToGenerate;
    }

    private static AttributeSyntax GetAttribute(TypeDeclarationSyntax tds)
    {
        foreach (AttributeListSyntax al in tds.AttributeLists)
        {
            foreach (AttributeSyntax attr in al.Attributes)
            {
                string n = attr.Name.ToString();
                if (n == "StrongType"
                    || n == "StrongTypeAttribute")
                {
                    return attr;
                }
            }
        }

        return null;
    }

    public static string? GetNamespace(Compilation compilation, TypeDeclarationSyntax tds)
    {
        SemanticModel semanticModel = compilation.GetSemanticModel(tds.SyntaxTree);

        // Get the symbol for the type declaration.
        if (semanticModel.GetDeclaredSymbol(tds) is INamedTypeSymbol typeSymbol)
        {
            // Get the fully qualified namespace as a string.
            string namespaceName = typeSymbol.ContainingNamespace.ToDisplayString();
            return namespaceName;
        }

        return string.Empty;
    }


    private static void Execute(Compilation compilation, ImmutableArray<TypeDeclarationSyntax> typeDeclarations, SourceProductionContext context)
    {
        sourceProductionContext = context;

        List<TypeToGenerate> endpointsToGenerate = GetTypesToGenerate(compilation, typeDeclarations.Distinct(), context.CancellationToken);

        foreach (TypeToGenerate typeToGenerate in endpointsToGenerate)
        {
            string source = SourceGenerator.Generate(typeToGenerate);
            context.AddSource($"{typeToGenerate.Name}.g.cs", source);
        }
    }

    private static TypeDeclarationSyntax? GetSemanticTargetForGeneration(GeneratorSyntaxContext context)
    {
        // we know the node is a EnumDeclarationSyntax thanks to IsSyntaxTargetForGeneration
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
}