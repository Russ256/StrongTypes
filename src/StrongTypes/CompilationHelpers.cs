namespace StrongTypes;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

internal static class CompilationHelpers
{
    internal static AttributeSyntax GetAttribute(this Compilation compilation, TypeDeclarationSyntax typeDeclaration, string name)
    {
        string nameWithAttribute = name.EndsWith("Attribute") ? name : $"{name}Attribute";

        foreach (AttributeListSyntax attributeListSyntax in typeDeclaration.AttributeLists)
        {
            foreach (AttributeSyntax attributeSyntax in attributeListSyntax.Attributes)
            {
                string n = attributeSyntax.Name.ToString();
                string ns = compilation.Namespace(attributeSyntax);

                if (ns == Generator._namespace && (n == name || n == nameWithAttribute))
                {
                    return attributeSyntax;
                }
            }
        }

        return null;
    }

    internal static string Namespace(this Compilation compilation, TypeDeclarationSyntax typeDeclaration)
    {
        return compilation.GetSemanticModel(typeDeclaration.SyntaxTree)
                          .GetDeclaredSymbol(typeDeclaration)?
                          .ContainingNamespace?
                          .ToString()
                          ?? string.Empty;
    }

    internal static string Namespace(this Compilation compilation, SyntaxNode syntaxNode)
    {
        return compilation.GetSemanticModel(syntaxNode.SyntaxTree)
                          .GetSymbolInfo(syntaxNode)
                          .Symbol?
                          .ContainingNamespace?
                          .ToString()
                          ?? string.Empty;
    }
}