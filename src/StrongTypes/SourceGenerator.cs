namespace StrongTypes;

using System;
using System.Text;

public class SourceGenerator
{
    public static string Generate(TypeToGenerate typeToGenerate)
    {
        return typeToGenerate.BaseType switch
        {
            "System.Guid" => GenerateGuid(typeToGenerate),
            "int" => GenerateInt(typeToGenerate),
            _ => throw new ArgumentException($"Type {typeToGenerate.BaseType} is not supported"),
        };
    }

    private static string GenerateInt(TypeToGenerate typeToGenerate)
    {
        string code = $$"""
                        // Auto-generated code 
                        namespace {{typeToGenerate.Namespace}};
                        
                        using System.Diagnostics.CodeAnalysis;
                        using System.Text.Json.Serialization;
                        using System.Text.Json;
                         
                        [JsonConverter(typeof(JsonConverter))]
                        public readonly partial struct {{typeToGenerate.Name}} : IComparable<{{typeToGenerate.Name}}>, IParsable<{{typeToGenerate.Name}}>
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
                        
                            public override bool Equals([NotNullWhen(true)] object? o)
                            {
                                if (o is {{typeToGenerate.Name}} other)
                                    return _value == other._value;
                        
                                return false;
                            }
                        
                            public override int GetHashCode() => _value.GetHashCode();
                            public static bool operator == ({{typeToGenerate.Name}} a, {{typeToGenerate.Name}} b) => a._value == b._value;
                            public static bool operator != ({{typeToGenerate.Name}} a, {{typeToGenerate.Name}} b) => a._value != b._value;
                        
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

    private static string GenerateGuid(TypeToGenerate typeToGenerate)
    {
        string code = $$"""
                // Auto-generated code 
                namespace {{typeToGenerate.Namespace}};

                using System.Diagnostics.CodeAnalysis;
                using System.Text.Json.Serialization;
                using System.Text.Json;
                
                [JsonConverter(typeof(JsonConverter))]
                public readonly partial struct {{typeToGenerate.Name}} : IComparable<{{typeToGenerate.Name}}>, IParsable<{{typeToGenerate.Name}}>
                {
                    private readonly {{typeToGenerate.BaseType}} _value;

                    public {{typeToGenerate.Name}}({{typeToGenerate.BaseType}} value) => _value = value;
                    public {{typeToGenerate.Name}}(string value)
                    {
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
                    public static {{typeToGenerate.Name}} Empty() => new {{typeToGenerate.Name}}(Guid.Empty);

                    public override string ToString() => _value.ToString();

                    public int CompareTo({{typeToGenerate.Name}} other) => _value.CompareTo(other._value);

                    public override bool Equals([NotNullWhen(true)] object? o)
                    {
                        if (o is {{typeToGenerate.Name}} other)
                            return _value == other._value;

                        return false;
                    }

                    public override int GetHashCode() => _value.GetHashCode();
                    public static bool operator == ({{typeToGenerate.Name}} a, {{typeToGenerate.Name}} b) => a._value == b._value;
                    public static bool operator != ({{typeToGenerate.Name}} a, {{typeToGenerate.Name}} b) => a._value != b._value;

                    public class JsonConverter : JsonConverter<{{typeToGenerate.Name}}>
                    {
                        public override {{typeToGenerate.Name}} Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
                            => new {{typeToGenerate.Name}}(reader.GetString());

                        public override void Write(Utf8JsonWriter writer, {{typeToGenerate.Name}} value, JsonSerializerOptions options)
                            => writer.WriteStringValue(value._value);
                    }
                }
                """;

        return code;
    }
}