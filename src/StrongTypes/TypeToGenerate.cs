namespace StrongTypes;

using Microsoft.CodeAnalysis.CSharp.Syntax;

internal struct TypeToGenerate
{
    internal string BaseTypeName { get; set; }

    internal readonly string ClassOrStruct
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

    internal TypeDeclarationSyntax DeclarationSyntax { get; set; }
    internal readonly bool IsClass => DeclarationSyntax is ClassDeclarationSyntax;
    internal readonly bool IsRecord => DeclarationSyntax is RecordDeclarationSyntax;
    internal readonly string Modifiers => DeclarationSyntax.Modifiers.ToString();
    internal readonly string Name => DeclarationSyntax.Identifier.Text;
    internal string Namespace { get; set; }
    internal int MaxLength { get; set; }
    internal bool IsEmail { get; set; }
}