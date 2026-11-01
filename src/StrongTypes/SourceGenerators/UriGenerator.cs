namespace StrongTypes.SourceGenerators;

internal static class UriGenerator
{
    internal static string Generate(TypeToGenerate typeToGenerate)
    {
        string code = $$"""
                {{SourceGenerator.GenerateHeader(typeToGenerate)}}

                [global::System.Text.Json.Serialization.JsonConverterAttribute(typeof(JsonConverter))]
                {{typeToGenerate.Modifiers}} class {{typeToGenerate.Name}}
                {
                    private readonly {{typeToGenerate.BaseTypeName}} _value;
                    public {{typeToGenerate.BaseTypeName}} Value => _value;

                    public {{typeToGenerate.Name}}({{typeToGenerate.BaseTypeName}} value)
                    {
                        global::System.ArgumentNullException.ThrowIfNull(value, nameof(value));

                        _value = value;
                    }

                    public {{typeToGenerate.Name}}(string value)
                    {
                        global::System.ArgumentNullException.ThrowIfNull(value, nameof(value));

                        _value = new global::System.Uri(value);
                    }

                    public override string ToString() => _value.ToString();

                    {{SourceGenerator.GenerateHash(typeToGenerate)}}
                    {{SourceGenerator.GenerateEquals(typeToGenerate)}}
                    {{SourceGenerator.GenerateEqualOperators(typeToGenerate)}}

                    public class JsonConverter : global::System.Text.Json.Serialization.JsonConverter<{{typeToGenerate.Name}}>
                    {
                        public override {{typeToGenerate.Name}} Read(ref global::System.Text.Json.Utf8JsonReader reader, global::System.Type typeToConvert, global::System.Text.Json.JsonSerializerOptions options)
                            => new {{typeToGenerate.Name}}(reader.GetString()!);

                        public override void Write(global::System.Text.Json.Utf8JsonWriter writer, {{typeToGenerate.Name}} value, global::System.Text.Json.JsonSerializerOptions options)
                            => writer.WriteStringValue(value._value.ToString());
                    }
                }
                """;

        return code;
    }
}
