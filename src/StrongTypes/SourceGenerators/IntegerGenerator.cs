namespace StrongTypes.SourceGenerators;

internal static class IntegerGenerator
{
    internal static string Generate(TypeToGenerate typeToGenerate)
    {
        string code = $$"""
                        {{SourceGenerator.GenerateHeader(typeToGenerate)}}

                        [global::System.Text.Json.Serialization.JsonConverterAttribute(typeof({{typeToGenerate.ConverterName}}))]
                        {{SourceGenerator.GenerateType(typeToGenerate)}}
                        {
                            private readonly {{typeToGenerate.BaseTypeName}} {{typeToGenerate.ValueFieldName}};
                            public {{typeToGenerate.BaseTypeName}} Value => {{typeToGenerate.ValueFieldName}};

                            public {{typeToGenerate.Name}}({{typeToGenerate.BaseTypeName}} value) => {{typeToGenerate.ValueFieldName}} = value;
                            public {{typeToGenerate.Name}}(string value)
                            {
                                if (!{{typeToGenerate.BaseTypeName}}.TryParse(value, out {{typeToGenerate.BaseTypeName}} parsedValue))
                                    throw new global::System.ArgumentException($"{value} is not a valid {{typeToGenerate.Name}}");
                                {{typeToGenerate.ValueFieldName}} = parsedValue;
                            }

                            public static {{typeToGenerate.Name}} Parse(string s) => Parse(s, provider: null);
                            public static {{typeToGenerate.Name}} Parse(string s, global::System.IFormatProvider? provider)
                            {
                                if (TryParse(s, provider, out {{typeToGenerate.NullableName}} result))
                                {
                                    return result;
                                }

                                throw new global::System.FormatException($"'{s}' is not a valid {{typeToGenerate.Name}}.");
                            }

                            public static bool TryParse(string? s, [global::System.Diagnostics.CodeAnalysis.NotNullWhenAttribute(true)] out {{typeToGenerate.NullableName}} result) => TryParse(s, provider: null, out result);
                            public static bool TryParse([global::System.Diagnostics.CodeAnalysis.NotNullWhenAttribute(true)] string? s, global::System.IFormatProvider? provider, [global::System.Diagnostics.CodeAnalysis.NotNullWhenAttribute(true)] out {{typeToGenerate.NullableName}} result)
                            {
                                result = default;

                                if (string.IsNullOrEmpty(s))
                                {
                                    return false;
                                }

                                if ({{typeToGenerate.BaseTypeName}}.TryParse(s, global::System.Globalization.NumberStyles.Integer, provider, out {{typeToGenerate.BaseTypeName}} parsedValue))
                                {
                                    result = new {{typeToGenerate.Name}}(parsedValue);
                                    return true;
                                }

                                return false;
                            }

                            public override string ToString() => {{typeToGenerate.ValueFieldName}}.ToString();

                            {{SourceGenerator.GenerateCompareTo(typeToGenerate)}}

                            {{SourceGenerator.GenerateHash(typeToGenerate)}}
                            {{SourceGenerator.GenerateEquals(typeToGenerate)}}
                            {{SourceGenerator.GenerateEqualOperators(typeToGenerate)}}

                            public class {{typeToGenerate.ConverterName}} : global::System.Text.Json.Serialization.JsonConverter<{{typeToGenerate.Name}}>
                            {
                                public override {{typeToGenerate.Name}} Read(ref global::System.Text.Json.Utf8JsonReader reader, global::System.Type typeToConvert, global::System.Text.Json.JsonSerializerOptions options)
                                    => new {{typeToGenerate.Name}}(reader.GetInt32());

                                public override void Write(global::System.Text.Json.Utf8JsonWriter writer, {{typeToGenerate.Name}} value, global::System.Text.Json.JsonSerializerOptions options)
                                    => writer.WriteNumberValue(value.{{typeToGenerate.ValueFieldName}});
                            }
                        }
                        """;

        return code;
    }

}
