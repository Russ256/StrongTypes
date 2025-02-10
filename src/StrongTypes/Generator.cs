namespace StrongTypes;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading;

[Generator(LanguageNames.CSharp)]
public class Generator : IIncrementalGenerator
{
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        // Add the our attributes to the compilation
        context.RegisterPostInitializationOutput(ctx => ctx.AddSource(
             "StringTypes.Attributes.g.cs",
           SourceText.From(Attributes.Source, Encoding.UTF8)));

        // Create a simple filter for class
        IncrementalValuesProvider<StructDeclarationSyntax> enumDeclarations = context.SyntaxProvider
            .CreateSyntaxProvider(
                predicate: static (s, _) => IsSyntaxTargetForGeneration(s), // select classes with attributes
                transform: static (ctx, _) => GetSemanticTargetForGeneration(ctx)) // sect the enum with the [MinApiGet] attribute
            .Where(static m => m is not null)!; // filter out attributed classes that we don't care about

        IncrementalValueProvider<(Compilation, ImmutableArray<StructDeclarationSyntax>)> compilationAndClasses
          = context.CompilationProvider.Combine(enumDeclarations.Collect());

        // Generate the source using the compilation and enums
        context.RegisterSourceOutput(compilationAndClasses,
            static (spc, source) => Execute(source.Item1, source.Item2, spc));
    }

    private static bool IsSyntaxTargetForGeneration(SyntaxNode node)
    {
        return node is StructDeclarationSyntax m && m.AttributeLists.Count > 0;
    }

    private static List<TypeToGenerate> GetTypesToGenerate(Compilation compilation, IEnumerable<StructDeclarationSyntax> structs, CancellationToken ct)
    {
        List<TypeToGenerate> typesToGenerate = [];

        foreach (StructDeclarationSyntax sts in structs)
        {
            ct.ThrowIfCancellationRequested();

            typesToGenerate.Add(new TypeToGenerate()
            {
                Name = sts.Identifier.Text,
                Namespace = GetNamespace(compilation, sts),
                BaseType = "Guid"
            });
        }

        return typesToGenerate;
    }

    public static string? GetNamespace(Compilation compilation, TypeDeclarationSyntax syntaxNode)
    {
        SemanticModel semanticModel = compilation.GetSemanticModel(syntaxNode.SyntaxTree);

        // Get the symbol for the type declaration.
        if (semanticModel.GetDeclaredSymbol(syntaxNode) is INamedTypeSymbol typeSymbol)
        {
            // Get the fully qualified namespace as a string.
            string namespaceName = typeSymbol.ContainingNamespace.ToDisplayString();
            return namespaceName;
        }

        return string.Empty;
    }


    private static void Execute(Compilation compilation, ImmutableArray<StructDeclarationSyntax> structs, SourceProductionContext context)
    {
        List<TypeToGenerate> endpointsToGenerate = GetTypesToGenerate(compilation, structs.Distinct(), context.CancellationToken);

        foreach (TypeToGenerate typeToGenerate in endpointsToGenerate)
        {
            string source = SourceGenerator.Generate(typeToGenerate);
            context.AddSource($"{typeToGenerate.Name}.g.cs", source);
        }
    }

    private static StructDeclarationSyntax? GetSemanticTargetForGeneration(GeneratorSyntaxContext context)
    {
        // we know the node is a EnumDeclarationSyntax thanks to IsSyntaxTargetForGeneration
        StructDeclarationSyntax structDeclarationSyntax = (StructDeclarationSyntax)context.Node;

        // loop through all the attributes on the method
        foreach (AttributeListSyntax attributeListSyntax in structDeclarationSyntax.AttributeLists)
        {
            foreach (AttributeSyntax attributeSyntax in attributeListSyntax.Attributes)
            {
                ISymbol attributeSymbol = context.SemanticModel.GetSymbolInfo(attributeSyntax).Symbol;

                INamedTypeSymbol attributeContainingTypeSymbol = attributeSymbol.ContainingType;
                string fullName = attributeContainingTypeSymbol.ToDisplayString();

                // Is the attribute the [EnumExtensions] attribute?
                if ((fullName == "StrongTypes.GuidAttribute")
                    )
                {
                    // Found it!
                    return structDeclarationSyntax;
                }
            }
        }

        // Nothing found
        return null;
    }
}