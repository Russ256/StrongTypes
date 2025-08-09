namespace StrongTypes.SourceGenerators;

internal static class GuidGenerator
{
    public static string Generate(TypeToGenerate typeToGenerate)
    {
        string code = $$"""
                {{SourceGenerator.GenerateHeader(typeToGenerate)}}

                [JsonConverter(typeof(JsonConverter))]
                {{SourceGenerator.GenerateType(typeToGenerate)}}
                {
                    private readonly {{typeToGenerate.BaseTypeName}} _value;
                    public {{typeToGenerate.BaseTypeName}} Value => _value;

                    public {{typeToGenerate.Name}}({{typeToGenerate.BaseTypeName}} value) => _value = value;
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

                    {{SourceGenerator.GenerateHash()}}
                    {{SourceGenerator.GenerateEquals(typeToGenerate)}}
                    {{SourceGenerator.GenerateEqualOperators(typeToGenerate)}}

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
}