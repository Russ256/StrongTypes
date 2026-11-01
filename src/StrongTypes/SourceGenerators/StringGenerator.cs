namespace StrongTypes.SourceGenerators;

internal static class StringGenerator
{
    internal static string Generate(TypeToGenerate typeToGenerate)
    {
        string code = $$"""
                {{SourceGenerator.GenerateHeader(typeToGenerate)}}

                [global::System.Text.Json.Serialization.JsonConverterAttribute(typeof({{typeToGenerate.ConverterName}}))]
                {{typeToGenerate.Modifiers}} class {{typeToGenerate.Name}} : global::System.IEquatable<{{typeToGenerate.Name}}>
                {
                    private readonly {{typeToGenerate.BaseTypeName}} {{typeToGenerate.ValueFieldName}};
                    public {{typeToGenerate.BaseTypeName}} Value => {{typeToGenerate.ValueFieldName}};

                    public {{typeToGenerate.Name}}({{typeToGenerate.BaseTypeName}} value)
                    {
                        global::System.ArgumentNullException.ThrowIfNull(value, nameof(value));

                        {{GenerateStringLengthValidation(typeToGenerate)}}
                        {{GenerateEmailValidation(typeToGenerate)}}

                        {{typeToGenerate.ValueFieldName}} = value;
                    }

                    public override string ToString() => {{typeToGenerate.ValueFieldName}}.ToString();


                    // Equality operators
                    public bool Equals([global::System.Diagnostics.CodeAnalysis.NotNullWhenAttribute(true)] {{typeToGenerate.Name}}? other)
                        => other is not null && string.Equals({{typeToGenerate.ValueFieldName}}, other.{{typeToGenerate.ValueFieldName}}, global::System.StringComparison.Ordinal);

                    public override bool Equals(object? obj)
                        => obj is {{typeToGenerate.Name}} t && Equals(t);

                    public override int GetHashCode()
                        => global::System.StringComparer.Ordinal.GetHashCode({{typeToGenerate.ValueFieldName}});

                    public static bool operator ==({{typeToGenerate.Name}}? left, {{typeToGenerate.Name}}? right)
                        => ReferenceEquals(left, right) || (left is not null && left.Equals(right));

                    public static bool operator !=({{typeToGenerate.Name}}? left, {{typeToGenerate.Name}}? right)
                        => !(left == right);

                    // null safe conversions
                    public static implicit operator string?({{typeToGenerate.Name}}? s)
                        => s?.{{typeToGenerate.ValueFieldName}};

                    public static implicit operator {{typeToGenerate.Name}}? (string? s)
                         => s is null ? null : new {{typeToGenerate.Name}}(s);

                    // Json serialization support
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
                            throw new global::StrongTypes.MaxLengthException(typeof({{typeToGenerate.Name}}), {{typeToGenerate.MaxLength}}, value.Length);
                        }

                """;
    }

    private static string GenerateEmailValidation(TypeToGenerate typeToGenerate)
    {
        if (!typeToGenerate.IsEmail)
        {
            return "";
        }

        return $$"""
                        // Email validation
                        if (value.Length == 0)
                        {
                            throw new global::StrongTypes.EmailAddressFormatException();
                        }

                        try
                        {
                            _ = new global::System.Net.Mail.MailAddress(value);
                        }
                        catch (global::System.FormatException)
                        {
                            throw new global::StrongTypes.EmailAddressFormatException();
                        }

                """;
    }

}
