namespace StrongTypes.Generator;

using System.Text;

public class SourceGenerator
{
    public static string Generate(TypeToGenerate typeToGenerate)
    {
        string start = $$"""
                // Auto-generated code 
                namespace {{typeToGenerate.Namespace}};

                using System.Text.Json.Serialization;
                using System.Text.Json;
                                                
                //[JsonConverter(typeof(JsonConverter))]
                public readonly partial struct {{typeToGenerate.Name}}
                {
                    private readonly {{typeToGenerate.BaseType}} _value;

                    public {{typeToGenerate.Name}}({{typeToGenerate.BaseType}} value) => _value = value;
                    public {{typeToGenerate.Name}}(string value)
                    {
                        if (!Guid.TryParse(value, out Guid guid))
                            throw new ArgumentException($"{value} is not a valid {{typeToGenerate.Name}}");
                        _value = guid;
                    }
                
                    public static {{typeToGenerate.Name}} New() => new {{typeToGenerate.Name}}(Guid.NewGuid());
                    public static {{typeToGenerate.Name}} Empty() => new {{typeToGenerate.Name}}(Guid.Empty);

                    public override string ToString() => _value.ToString();

                    //public class JsonConverter : JsonConverter<{{typeToGenerate.Name}}>
                    //{
                    //    public override TenantId Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
                    //        => Guid.ParseExact(reader.GetString(), "D");

                    //    public override void Write(Utf8JsonWriter writer, TenantId value, JsonSerializerOptions options)
                    //        => writer.WriteStringValue(({{typeToGenerate.BaseType}})value);
                    //}

                """;

        string end = $$"""
                }
                """;

        StringBuilder sb = new(start);

        sb.AppendLine(end);

        return sb.ToString();
    }
}