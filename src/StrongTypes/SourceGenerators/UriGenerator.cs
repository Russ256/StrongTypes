namespace StrongTypes.SourceGenerators;

internal static class UriGenerator
{
    internal static string Generate(TypeToGenerate typeToGenerate)
    {
        string code = $$"""
                {{SourceGenerator.GenerateHeader(typeToGenerate)}}

                [JsonConverter(typeof(JsonConverter))]
                {{typeToGenerate.Modifiers}} class {{typeToGenerate.Name}}
                {
                    private readonly {{typeToGenerate.BaseTypeName}} _value;
                    public {{typeToGenerate.BaseTypeName}} Value => _value;

                    public {{typeToGenerate.Name}}({{typeToGenerate.BaseTypeName}} value)
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

                    {{SourceGenerator.GenerateHash()}}
                    {{SourceGenerator.GenerateEquals(typeToGenerate)}}
                    {{SourceGenerator.GenerateEqualOperators(typeToGenerate)}}

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