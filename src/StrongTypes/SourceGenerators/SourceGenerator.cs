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

                    return a.{{typeToGenerate.ValueFieldName}} == b.{{typeToGenerate.ValueFieldName}};
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

                    return a.{{typeToGenerate.ValueFieldName}} != b.{{typeToGenerate.ValueFieldName}};
                }
                """;
        }

        return $$"""
                public static bool operator == ({{typeToGenerate.Name}} a, {{typeToGenerate.Name}} b) => a.{{typeToGenerate.ValueFieldName}} == b!.{{typeToGenerate.ValueFieldName}};
                public static bool operator != ({{typeToGenerate.Name}} a, {{typeToGenerate.Name}} b) => a.{{typeToGenerate.ValueFieldName}} != b.{{typeToGenerate.ValueFieldName}};
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
                 public bool Equals({{typeToGenerate.NullableName}} other)
                     => {{(typeToGenerate.IsReferenceType ? "other is not null && " : string.Empty)}}{{typeToGenerate.ValueFieldName}} == other.{{typeToGenerate.ValueFieldName}};

                 public override bool Equals([global::System.Diagnostics.CodeAnalysis.NotNullWhenAttribute(true)] object? o)
                 {
                     if (o is {{typeToGenerate.Name}} other)
                         return Equals(other);

                     return false;
                 }
                 """;
    }

    internal static string GenerateCompareTo(TypeToGenerate typeToGenerate)
    {
        if (typeToGenerate.IsRecord)
        {
            // Record equality includes user state and runtime type. There is no
            // general ordering for that state; consumers must supply a comparer.
            return string.Empty;
        }

        return typeToGenerate.IsReferenceType
            ? $"public int CompareTo({typeToGenerate.NullableName} other) => other is null ? 1 : {typeToGenerate.ValueFieldName}.CompareTo(other.{typeToGenerate.ValueFieldName});"
            : $"public int CompareTo({typeToGenerate.Name} other) => {typeToGenerate.ValueFieldName}.CompareTo(other.{typeToGenerate.ValueFieldName});";
    }

    internal static string GenerateHash(TypeToGenerate typeToGenerate)
    {
        if (typeToGenerate.IsRecord)
        {
            return string.Empty;
        }

        return $$"""
                public override int GetHashCode() => {{typeToGenerate.ValueFieldName}}.GetHashCode();
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
        string interfaces = typeToGenerate.IsRecord
            ? string.Empty
            : $"global::System.IEquatable<{typeToGenerate.Name}>, global::System.IComparable<{typeToGenerate.Name}>, ";
        return $"{typeToGenerate.Modifiers}{(typeToGenerate.IsRecord ? " record " : " ")}{typeToGenerate.ClassOrStruct} {typeToGenerate.Name} : {interfaces}global::System.IParsable<{typeToGenerate.Name}>";
    }
}
