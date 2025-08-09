namespace StrongTypes.SourceGenerators;

internal static class IntegerGenerator
{
    internal static string Generate(TypeToGenerate typeToGenerate)
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
                                if (!{{typeToGenerate.BaseTypeName}}.TryParse(value, out {{typeToGenerate.BaseTypeName}} parsedValue))
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

                                if ({{typeToGenerate.BaseTypeName}}.TryParse(s, out {{typeToGenerate.BaseTypeName}} parsedValue))
                                {
                                    result = new {{typeToGenerate.Name}}(parsedValue);
                                    return true;
                                }

                                return false;
                            }

                            public override string ToString() => _value.ToString();

                            public int CompareTo({{typeToGenerate.Name}} other) => _value.CompareTo(other._value);

                            {{SourceGenerator.GenerateHash()}}
                            {{SourceGenerator.GenerateEquals(typeToGenerate)}}
                            {{SourceGenerator.GenerateEqualOperators(typeToGenerate)}}

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

}