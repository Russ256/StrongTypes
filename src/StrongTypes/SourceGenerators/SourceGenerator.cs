namespace StrongTypes.SourceGenerators;

internal static class SourceGenerator
{
    public static string Generate(TypeToGenerate typeToGenerate)
    {
        return typeToGenerate.BaseTypeName switch
        {
            "System.Uri" => UriGenerator.Generate(typeToGenerate),
            "System.Guid" => GuidGenerator.Generate(typeToGenerate),
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
                public static bool operator == ({{typeToGenerate.Name}} a, {{typeToGenerate.Name}} b)
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

                public static bool operator != ({{typeToGenerate.Name}} a, {{typeToGenerate.Name}} b)
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
            return $$"""
                     public bool Equals([NotNullWhen(true)] {{typeToGenerate.Name}} other) => _value == other._value;
                     """;
        }

        return $$"""
                 public override bool Equals([NotNullWhen(true)] object? o)
                 {
                     if (o is {{typeToGenerate.Name}} other)
                         return _value == other._value;

                     return false;
                 }
                 """;
    }

    internal static string GenerateHash()
    {
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

                using System.Diagnostics.CodeAnalysis;
                using System.Text.Json.Serialization;
                using System.Text.Json;
                """;
    }

    internal static string GenerateType(TypeToGenerate typeToGenerate)
    {
        return $"{typeToGenerate.Modifiers}{(typeToGenerate.IsRecord ? " record " : " ")}{typeToGenerate.ClassOrStruct} {typeToGenerate.Name} : IComparable<{typeToGenerate.Name}>, IParsable<{typeToGenerate.Name}>";
    }
}