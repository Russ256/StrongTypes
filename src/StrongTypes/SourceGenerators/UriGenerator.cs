namespace StrongTypes.SourceGenerators;

internal static class UriGenerator
{
    internal static string Generate(TypeToGenerate typeToGenerate)
    {
        string code = $$"""
                {{SourceGenerator.GenerateHeader(typeToGenerate)}}

                [global::System.Text.Json.Serialization.JsonConverterAttribute(typeof({{typeToGenerate.ConverterName}}))]
                {{typeToGenerate.Modifiers}} class {{typeToGenerate.Name}}
                {
                    private readonly {{typeToGenerate.BaseTypeName}} {{typeToGenerate.ValueFieldName}};
                    public {{typeToGenerate.BaseTypeName}} Value => {{typeToGenerate.ValueFieldName}};

                    public {{typeToGenerate.Name}}({{typeToGenerate.BaseTypeName}} value)
                    {
                        global::System.ArgumentNullException.ThrowIfNull(value, nameof(value));

                        {{typeToGenerate.ValueFieldName}} = value;
                    }

                    public {{typeToGenerate.Name}}(string value)
                    {
                        global::System.ArgumentNullException.ThrowIfNull(value, nameof(value));

                        {{typeToGenerate.ValueFieldName}} = new global::System.Uri(value);
                    }

                    public override string ToString() => {{typeToGenerate.ValueFieldName}}.ToString();

                    {{SourceGenerator.GenerateHash(typeToGenerate)}}
                    {{SourceGenerator.GenerateEquals(typeToGenerate)}}
                    {{SourceGenerator.GenerateEqualOperators(typeToGenerate)}}

                    public class {{typeToGenerate.ConverterName}} : global::System.Text.Json.Serialization.JsonConverter<{{typeToGenerate.Name}}>
                    {
                        public override {{typeToGenerate.Name}} Read(ref global::System.Text.Json.Utf8JsonReader reader, global::System.Type typeToConvert, global::System.Text.Json.JsonSerializerOptions options)
                            => new {{typeToGenerate.Name}}(reader.GetString()!);

                        public override void Write(global::System.Text.Json.Utf8JsonWriter writer, {{typeToGenerate.Name}} value, global::System.Text.Json.JsonSerializerOptions options)
                            => writer.WriteStringValue(value.{{typeToGenerate.ValueFieldName}}.ToString());
                    }
                }
                """;

        return code;
    }
}
