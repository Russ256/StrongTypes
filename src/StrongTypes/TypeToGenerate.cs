namespace StrongTypes;

using System;

// Only value data crosses the incremental caching boundary.
internal struct TypeToGenerate : IEquatable<TypeToGenerate>
{
    internal string BaseTypeName { get; set; }
    internal string HintName { get; set; }
    internal string ValueFieldName { get; set; }
    internal string ConverterName { get; set; }
    internal string ClassOrStruct { get; set; }
    internal bool IsClass { get; set; }
    internal bool IsRecord { get; set; }
    internal bool IsReferenceType { get; set; }
    internal readonly string NullableName => Name + (IsReferenceType ? "?" : string.Empty);
    internal string Modifiers { get; set; }
    internal string Name { get; set; }
    internal string Namespace { get; set; }
    internal int? MaxLength { get; set; }
    internal bool IsEmail { get; set; }

    public readonly bool Equals(TypeToGenerate other)
        => BaseTypeName == other.BaseTypeName
        && HintName == other.HintName
        && ValueFieldName == other.ValueFieldName
        && ConverterName == other.ConverterName
        && ClassOrStruct == other.ClassOrStruct
        && IsClass == other.IsClass
        && IsRecord == other.IsRecord
        && IsReferenceType == other.IsReferenceType
        && Modifiers == other.Modifiers
        && Name == other.Name
        && Namespace == other.Namespace
        && MaxLength == other.MaxLength
        && IsEmail == other.IsEmail;

    public override readonly bool Equals(object obj) => obj is TypeToGenerate other && Equals(other);

    public override readonly int GetHashCode() => HintName?.GetHashCode() ?? 0;
}