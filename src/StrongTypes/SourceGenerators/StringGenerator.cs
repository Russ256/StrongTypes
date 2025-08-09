namespace StrongTypes.SourceGenerators;

internal static class StringGenerator
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

                        {{GenerateStringLengthValidation(typeToGenerate)}}

                        _value = value;
                    }

                    public static implicit operator {{typeToGenerate.Name}}(string s)
                    {
                        return new {{typeToGenerate.Name}}(s);
                    }

                    public override string ToString() => _value.ToString();

                    // Euqality operators
                    public static bool operator ==({{typeToGenerate.Name}} left, {{typeToGenerate.Name}} right)
                    {
                        if (left is null && right is null)
                        {
                            return true;
                        }

                        if ((left is null && right is not null)
                             || (left is not null && right is null))
                        {
                            return false;
                        }

                        return left!.Value == right!.Value;
                    }

                    public static bool operator ==({{typeToGenerate.Name}} left, string right)
                    {
                        if (left is null && right is null)
                        {
                            return true;
                        }

                        if ((left is null && right is not null)
                            || (right is null && left is not null))
                        {
                            return false;
                        }

                        return left!.Value == right;
                    }

                    public static bool operator ==(string left, {{typeToGenerate.Name}} right)
                    {
                        if (left is null && right is null)
                        {
                            return true;
                        }

                        if ((left is null && right is not null)
                            || (right is null && left is not null))
                        {
                            return false;
                        }

                        return left == right!.Value;
                    }

                    public static bool operator !=({{typeToGenerate.Name}} left, {{typeToGenerate.Name}} right)
                    {
                        if (left is null && right is null)
                        {
                            return false;
                        }

                        if ((left is null && right is not null)
                             || (left is not null && right is null))
                        {
                            return true;
                        }

                        return left!.Value != right!.Value;
                    }

                    public static bool operator !=({{typeToGenerate.Name}} left, string right)
                    {
                        if (left is null && right is null)
                        {
                            return false;
                        }

                        if ((left is null && right is not null)
                            || (right is null && left is not null))
                        {
                            return true;
                        }

                        return left!.Value != right;
                    }

                    public static bool operator !=(string left, {{typeToGenerate.Name}} right)
                    {
                         if (left is null && right is null)
                        {
                            return false;
                        }

                        if ((left is null && right is not null)
                            || (right is null && left is not null))
                        {
                            return true;
                        }

                        return left != right!.Value;
                   }

                    {{SourceGenerator.GenerateHash()}}

                    public override bool Equals([NotNullWhen(true)] object? o)
                    {
                        if (o is {{typeToGenerate.Name}} other)
                            return _value == other._value;

                        return false;
                    }

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

    private static string GenerateStringLengthValidation(TypeToGenerate typeToGenerate)
    {
        if (typeToGenerate.MaxLength == 0)
        {
            return "";
        }

        return $$"""
                        // Max length check
                        if (value.Length > {{typeToGenerate.MaxLength}})
                        {
                            throw new StrongTypes.MaxLengthException(typeof({{typeToGenerate.Name}}), {{typeToGenerate.MaxLength}}, value.Length);
                        }

                """;
    }
}