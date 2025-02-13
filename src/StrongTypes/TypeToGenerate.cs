namespace StrongTypes;

using Microsoft.CodeAnalysis.CSharp.Syntax;

public struct TypeToGenerate
{
    public TypeDeclarationSyntax DeclarationSyntax { get; set; }

    public string BaseType { get; set; }

    public string ClassOrStruct
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

    public string Modifiers 
    { 
        get
        {
            return DeclarationSyntax.Modifiers.ToString();
        }
    }

    public string Name
    {
        get
        {
            return DeclarationSyntax.Identifier.Text;
        }
    }

    public string Namespace { get; set; }

    public readonly bool IsRecord => DeclarationSyntax is RecordDeclarationSyntax;
}