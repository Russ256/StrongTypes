namespace StrongTypes.Generator;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Immutable;

[Generator(LanguageNames.CSharp)]
public class Generator : IIncrementalGenerator
{
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
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


    private static void Execute(Compilation compilation, ImmutableArray<StructDeclarationSyntax> structs, SourceProductionContext context)
    {
    }

    private static StructDeclarationSyntax? GetSemanticTargetForGeneration(GeneratorSyntaxContext context)
    {
        // we know the node is a EnumDeclarationSyntax thanks to IsSyntaxTargetForGeneration
        StructDeclarationSyntax structodDeclarationSyntax = (StructDeclarationSyntax)context.Node;

        // loop through all the attributes on the method
        foreach (AttributeListSyntax attributeListSyntax in structodDeclarationSyntax.AttributeLists)
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
                    return structodDeclarationSyntax;
                }
            }
        }

        // Nothing found
        return null;
    }
}