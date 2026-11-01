namespace StrongTypes.SourceGenerators;

internal static class SourceGenerator
{
    public static string Generate(TypeToGenerate typeToGenerate)
    {
        return typeToGenerate.BaseTypeName switch
        {
            "global::System.Uri" => UriGenerator.Generate(typeToGenerate),
            "global::System.Guid" => GuidGenerator.Generate(typeToGenerate),
            "int" => IntegerGenerator.Generate(typeToGenerate),
            "string" => StringGenerator.Generate(typeToGenerate),
            _ => null,
        };
    }

    internal static string GenerateEqualOperators(TypeToGenerate typeToGenerate)
    {
        if (typeToGenerate.IsRecord)
        {
            return string.Empty;
        }

        if (typeToGenerate.IsClass)
        {
            return $$"""
                public static bool operator == ({{typeToGenerate.NullableName}} a, {{typeToGenerate.NullableName}} b)
                {
                    if (a is null && b is null)
                    {
                        return true;
                    }

                    if (a is null || b is null)
                    {
                        return false;
                    }

                    return a._value == b._value;
                }

                public static bool operator != ({{typeToGenerate.NullableName}} a, {{typeToGenerate.NullableName}} b)
                {
                    if (a is null && b is null)
                    {
                        return false;
                    }

                    if (a is null || b is null)
                    {
                        return true;
                    }

                    return a._value != b._value;
                }
                """;
        }

        return $$"""
                public static bool operator == ({{typeToGenerate.Name}} a, {{typeToGenerate.Name}} b) => a._value == b!._value;
                public static bool operator != ({{typeToGenerate.Name}} a, {{typeToGenerate.Name}} b) => a._value != b._value;
                """;
    }

    internal static string GenerateEquals(TypeToGenerate typeToGenerate)
    {
        if (typeToGenerate.IsRecord)
        {
            // Let the compiler synthesize null-safe equality, including record
            // inheritance contracts and any additional user-declared state.
            return string.Empty;
        }

        return $$"""
                 public override bool Equals([global::System.Diagnostics.CodeAnalysis.NotNullWhenAttribute(true)] object? o)
                 {
                     if (o is {{typeToGenerate.Name}} other)
                         return _value == other._value;

                     return false;
                 }
                 """;
    }

    internal static string GenerateCompareTo(TypeToGenerate typeToGenerate)
    {
        return typeToGenerate.IsReferenceType
            ? $"public int CompareTo({typeToGenerate.NullableName} other) => other is null ? 1 : _value.CompareTo(other._value);"
            : $"public int CompareTo({typeToGenerate.Name} other) => _value.CompareTo(other._value);";
    }

    internal static string GenerateHash(TypeToGenerate typeToGenerate)
    {
        if (typeToGenerate.IsRecord)
        {
            return string.Empty;
        }

        return $$"""
                public override int GetHashCode() => _value.GetHashCode();
                """;
    }

    internal static string GenerateHeader(TypeToGenerate typeToGenerate)
    {
        return $$"""
                // Auto-generated code
                #nullable enable

                namespace {{typeToGenerate.Namespace}};
                """;
    }

    internal static string GenerateType(TypeToGenerate typeToGenerate)
    {
        return $"{typeToGenerate.Modifiers}{(typeToGenerate.IsRecord ? " record " : " ")}{typeToGenerate.ClassOrStruct} {typeToGenerate.Name} : global::System.IComparable<{typeToGenerate.Name}>, global::System.IParsable<{typeToGenerate.Name}>";
    }
}
