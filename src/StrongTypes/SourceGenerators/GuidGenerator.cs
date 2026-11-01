namespace StrongTypes.SourceGenerators;

internal static class GuidGenerator
{
    public static string Generate(TypeToGenerate typeToGenerate)
    {
        string code = $$"""
                {{SourceGenerator.GenerateHeader(typeToGenerate)}}

                [global::System.Text.Json.Serialization.JsonConverterAttribute(typeof(JsonConverter))]
                {{SourceGenerator.GenerateType(typeToGenerate)}}
                {
                    private readonly {{typeToGenerate.BaseTypeName}} _value;
                    public {{typeToGenerate.BaseTypeName}} Value => _value;

                    public {{typeToGenerate.Name}}({{typeToGenerate.BaseTypeName}} value) => _value = value;
                    public {{typeToGenerate.Name}}(string value)
                    {
                        global::System.ArgumentNullException.ThrowIfNull(value, nameof(value));

                        if (!global::System.Guid.TryParse(value, out global::System.Guid guid))
                            throw new global::System.ArgumentException($"{value} is not a valid {{typeToGenerate.Name}}");

                        _value = guid;
                    }

                    public static {{typeToGenerate.Name}} Parse(string s) => Parse(s, provider: null);
                    public static {{typeToGenerate.Name}} Parse(string s, global::System.IFormatProvider? provider)
                    {
                        if (TryParse(s, provider, out {{typeToGenerate.Name}} result))
                        {
                            return result;
                        }

                        throw new global::System.FormatException($"'{s}' is not a valid {{typeToGenerate.Name}}.");
                    }

                    public static bool TryParse(string? s, out {{typeToGenerate.Name}} result) => TryParse(s, provider: null, out result);
                    public static bool TryParse([global::System.Diagnostics.CodeAnalysis.NotNullWhenAttribute(true)] string? s, global::System.IFormatProvider? provider, [global::System.Diagnostics.CodeAnalysis.MaybeNullWhenAttribute(false)] out {{typeToGenerate.Name}} result)
                    {
                        result = default;

                        if (string.IsNullOrEmpty(s))
                        {
                            return false;
                        }

                        if (global::System.Guid.TryParse(s, out global::System.Guid guid))
                        {
                            result = new {{typeToGenerate.Name}}(guid);
                            return true;
                        }

                        return false;
                    }

                    public static {{typeToGenerate.Name}} New() => new {{typeToGenerate.Name}}(global::System.Guid.NewGuid());
                    public static readonly {{typeToGenerate.Name}} Empty = new {{typeToGenerate.Name}}(global::System.Guid.Empty);

                    public override string ToString() => _value.ToString();

                    public int CompareTo({{typeToGenerate.Name}} other) => _value.CompareTo(other._value);

                    {{SourceGenerator.GenerateHash(typeToGenerate)}}
                    {{SourceGenerator.GenerateEquals(typeToGenerate)}}
                    {{SourceGenerator.GenerateEqualOperators(typeToGenerate)}}

                    public class JsonConverter : global::System.Text.Json.Serialization.JsonConverter<{{typeToGenerate.Name}}>
                    {
                        public override {{typeToGenerate.Name}} Read(ref global::System.Text.Json.Utf8JsonReader reader, global::System.Type typeToConvert, global::System.Text.Json.JsonSerializerOptions options)
                            => new {{typeToGenerate.Name}}(reader.GetString()!);

                        public override void Write(global::System.Text.Json.Utf8JsonWriter writer, {{typeToGenerate.Name}} value, global::System.Text.Json.JsonSerializerOptions options)
                            => writer.WriteStringValue(value._value);
                    }
                }
                """;

        return code;
    }
}
