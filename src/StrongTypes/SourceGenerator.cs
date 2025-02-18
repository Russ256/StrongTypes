namespace StrongTypes;

using System;

public class SourceGenerator
{
    public static string Generate(TypeToGenerate typeToGenerate)
    {
        return typeToGenerate.BaseType switch
        {
            "System.Uri" => GenerateUri(typeToGenerate),
            "System.Guid" => GenerateGuid(typeToGenerate),
            "int" => GenerateInt(typeToGenerate),
            _ => throw new ArgumentException($"Type {typeToGenerate.BaseType} is not supported"),
        };
    }

    private static string GenerateInt(TypeToGenerate typeToGenerate)
    {
        string code = $$"""
                        {{GenerateHeader(typeToGenerate)}}
                        
                        [JsonConverter(typeof(JsonConverter))]
                        {{GenerateType(typeToGenerate)}}
                        {
                            private readonly {{typeToGenerate.BaseType}} _value;
                            public {{typeToGenerate.BaseType}} Value => _value;

                            public {{typeToGenerate.Name}}({{typeToGenerate.BaseType}} value) => _value = value;
                            public {{typeToGenerate.Name}}(string value)
                            {
                                if (!{{typeToGenerate.BaseType}}.TryParse(value, out {{typeToGenerate.BaseType}} parsedValue))
                                    throw new ArgumentException($"{value} is not a valid {{typeToGenerate.Name}}");
                                _value = parsedValue;
                            }

                            public static {{typeToGenerate.Name}} Parse(string s) => Parse(s, provider: null);
                            public static {{typeToGenerate.Name}} Parse(string s, IFormatProvider? provider)
                            {
                                if (TryParse(s, provider, out {{typeToGenerate.Name}} result))
                                {
                                    return result;
                                }

                                throw new FormatException($"'{s}' is not a valid {{typeToGenerate.Name}}.");
                            }

                            public static bool TryParse(string? s, out {{typeToGenerate.Name}} result) => TryParse(s, provider: null, out result);
                            public static bool TryParse([NotNullWhen(true)] string? s, IFormatProvider? provider, [MaybeNullWhen(false)] out {{typeToGenerate.Name}} result)
                            {
                                result = default;

                                if (string.IsNullOrEmpty(s))
                                {
                                    return false;
                                }

                                if ({{typeToGenerate.BaseType}}.TryParse(s, out {{typeToGenerate.BaseType}} parsedValue))
                                {
                                    result = new {{typeToGenerate.Name}}(parsedValue);
                                    return true;
                                }

                                return false;
                            }

                            public override string ToString() => _value.ToString();

                            public int CompareTo({{typeToGenerate.Name}} other) => _value.CompareTo(other._value);

                            {{GenerateHash()}}
                            {{GenerateEquals(typeToGenerate)}}
                            {{GenerateEqualOperators(typeToGenerate)}}

                            public class JsonConverter : JsonConverter<{{typeToGenerate.Name}}>
                            {
                                public override {{typeToGenerate.Name}} Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
                                    => new {{typeToGenerate.Name}}(reader.GetInt32());

                                public override void Write(Utf8JsonWriter writer, {{typeToGenerate.Name}} value, JsonSerializerOptions options)
                                    => writer.WriteNumberValue(value._value);
                            }
                        }
                        """;

        return code;
    }

    private static string GenerateType(TypeToGenerate typeToGenerate)
    {
        return $"{typeToGenerate.Modifiers}{(typeToGenerate.IsRecord ? " record " : " ")}{typeToGenerate.ClassOrStruct} {typeToGenerate.Name} : IComparable<{typeToGenerate.Name}>, IParsable<{typeToGenerate.Name}>";
    }

    private static string GenerateEqualOperators(TypeToGenerate typeToGenerate)
    {
        if (typeToGenerate.IsRecord)
        {
            return string.Empty;
        }

        return $$"""
                public static bool operator == ({{typeToGenerate.Name}} a, {{typeToGenerate.Name}} b) => a._value == b._value;
                public static bool operator != ({{typeToGenerate.Name}} a, {{typeToGenerate.Name}} b) => a._value != b._value;
                """;
    }

    private static string GenerateEquals(TypeToGenerate typeToGenerate)
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

    private static string GenerateHash()
    {
        return $$"""
                public override int GetHashCode() => _value.GetHashCode();
                """;
    }

    private static string GenerateHeader(TypeToGenerate typeToGenerate)
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

    private static string GenerateGuid(TypeToGenerate typeToGenerate)
    {
        string code = $$"""
                {{GenerateHeader(typeToGenerate)}}

                [JsonConverter(typeof(JsonConverter))]
                {{GenerateType(typeToGenerate)}}
                {
                    private readonly {{typeToGenerate.BaseType}} _value;
                    public {{typeToGenerate.BaseType}} Value => _value;

                    public {{typeToGenerate.Name}}({{typeToGenerate.BaseType}} value) => _value = value;
                    public {{typeToGenerate.Name}}(string value)
                    {
                        ArgumentNullException.ThrowIfNull(value, nameof(value));

                        if (!Guid.TryParse(value, out Guid guid))
                            throw new ArgumentException($"{value} is not a valid {{typeToGenerate.Name}}");

                        _value = guid;
                    }

                    public static {{typeToGenerate.Name}} Parse(string s) => Parse(s, provider: null);
                    public static {{typeToGenerate.Name}} Parse(string s, IFormatProvider? provider)
                    {
                        if (TryParse(s, provider, out {{typeToGenerate.Name}} result))
                        {
                            return result;
                        }

                        throw new FormatException($"'{s}' is not a valid {{typeToGenerate.Name}}.");
                    }

                    public static bool TryParse(string? s, out {{typeToGenerate.Name}} result) => TryParse(s, provider: null, out result);
                    public static bool TryParse([NotNullWhen(true)] string? s, IFormatProvider? provider, [MaybeNullWhen(false)] out {{typeToGenerate.Name}} result)
                    {
                        result = default;

                        if (string.IsNullOrEmpty(s))
                        {
                            return false;
                        }

                        if (Guid.TryParse(s, out Guid guid))
                        {
                            result = new {{typeToGenerate.Name}}(guid);
                            return true;
                        }

                        return false;
                    }

                    public static {{typeToGenerate.Name}} New() => new {{typeToGenerate.Name}}(Guid.NewGuid());
                    public static readonly {{typeToGenerate.Name}} Empty = new {{typeToGenerate.Name}}();

                    public override string ToString() => _value.ToString();

                    public int CompareTo({{typeToGenerate.Name}} other) => _value.CompareTo(other._value);

                    {{GenerateHash()}}
                    {{GenerateEquals(typeToGenerate)}}
                    {{GenerateEqualOperators(typeToGenerate)}}

                    public class JsonConverter : JsonConverter<{{typeToGenerate.Name}}>
                    {
                        public override {{typeToGenerate.Name}} Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
                            => new {{typeToGenerate.Name}}(reader.GetString()!);

                        public override void Write(Utf8JsonWriter writer, {{typeToGenerate.Name}} value, JsonSerializerOptions options)
                            => writer.WriteStringValue(value._value);
                    }
                }
                """;

        return code;
    }

    private static string GenerateUri(TypeToGenerate typeToGenerate)
    {
        string code = $$"""
                {{GenerateHeader(typeToGenerate)}}
                
                [JsonConverter(typeof(JsonConverter))]
                {{typeToGenerate.Modifiers}} class {{typeToGenerate.Name}}
                {
                    private readonly {{typeToGenerate.BaseType}} _value;
                    public {{typeToGenerate.BaseType}} Value => _value;

                    public {{typeToGenerate.Name}}({{typeToGenerate.BaseType}} value)
                    {
                        ArgumentNullException.ThrowIfNull(value, nameof(value));
                
                        _value = value;
                    }

                    public {{typeToGenerate.Name}}(string value)
                    {
                        ArgumentNullException.ThrowIfNull(value, nameof(value));

                        _value = new Uri(value);
                    }

                    public override string ToString() => _value.ToString();

                    {{GenerateHash()}}
                    {{GenerateEquals(typeToGenerate)}}
                    {{GenerateEqualOperators(typeToGenerate)}}
                                
                    public class JsonConverter : JsonConverter<{{typeToGenerate.Name}}>
                    {
                        public override {{typeToGenerate.Name}} Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
                            => new {{typeToGenerate.Name}}(reader.GetString()!);

                        public override void Write(Utf8JsonWriter writer, {{typeToGenerate.Name}} value, JsonSerializerOptions options)
                            => writer.WriteStringValue(value._value.ToString());
                    }
                }
                """;

        return code;
    }
}