namespace StrongTypes;

using Microsoft.CodeAnalysis.CSharp.Syntax;

public struct TypeToGenerate
{
    public string BaseType { get; set; }

    public readonly string ClassOrStruct
    {
        get
        {
            if (DeclarationSyntax is RecordDeclarationSyntax recordDeclarationSyntax)
            {
                return recordDeclarationSyntax.ClassOrStructKeyword.Text;
            }
            else
            {
                return DeclarationSyntax.Keyword.Text;
            }
        }
    }

    public TypeDeclarationSyntax DeclarationSyntax { get; set; }
    public readonly bool IsRecord => DeclarationSyntax is RecordDeclarationSyntax;
    public readonly string Modifiers => DeclarationSyntax.Modifiers.ToString();
    public readonly string Name => DeclarationSyntax.Identifier.Text;
    public string Namespace { get; set; }
}