namespace StrongTypes;

using Microsoft.CodeAnalysis.CSharp.Syntax;

internal struct TypeToGenerate
{
    internal string BaseTypeName { get; set; }
    internal string HintName { get; set; }

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
    internal readonly bool IsReferenceType => IsClass
        || DeclarationSyntax is RecordDeclarationSyntax record && record.ClassOrStructKeyword.Text != "struct";
    internal readonly string NullableName => Name + (IsReferenceType ? "?" : string.Empty);
    internal readonly string Modifiers => DeclarationSyntax.Modifiers.ToString();
    internal readonly string Name => DeclarationSyntax.Identifier.Text;
    internal string Namespace { get; set; }
    internal int MaxLength { get; set; }
    internal bool IsEmail { get; set; }
}
