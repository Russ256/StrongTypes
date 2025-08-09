namespace StrongTypes.SourceGenerators;

internal static class StringGenerator
{
    internal static string Generate(TypeToGenerate typeToGenerate)
    {
        string code = $$"""
                {{SourceGenerator.GenerateHeader(typeToGenerate)}}

                [JsonConverter(typeof(JsonConverter))]
                {{typeToGenerate.Modifiers}} class {{typeToGenerate.Name}} : IEquatable<{{typeToGenerate.Name}}>
                {
                    private readonly {{typeToGenerate.BaseTypeName}} _value;
                    public {{typeToGenerate.BaseTypeName}} Value => _value;

                    public {{typeToGenerate.Name}}({{typeToGenerate.BaseTypeName}} value)
                    {
                        ArgumentNullException.ThrowIfNull(value, nameof(value));

                        {{GenerateStringLengthValidation(typeToGenerate)}}

                        _value = value;
                    }

                    public override string ToString() => _value.ToString();


                    // Equality operators
                    public bool Equals([NotNullWhen(true)] {{typeToGenerate.Name}}? other) 
                        => other is not null && string.Equals(_value, other._value, StringComparison.Ordinal);

                    public override bool Equals(object? obj)
                        => obj is {{typeToGenerate.Name}} t && Equals(t);

                    public override int GetHashCode()
                        => StringComparer.Ordinal.GetHashCode(_value);

                    public static bool operator ==({{typeToGenerate.Name}}? left, {{typeToGenerate.Name}}? right)
                        => ReferenceEquals(left, right) || (left is not null && left.Equals(right));

                    public static bool operator !=({{typeToGenerate.Name}}? left, {{typeToGenerate.Name}}? right) 
                        => !(left == right);
                  
                    // String interop - optional but avoids allocations (compiler will generate a new instance of the type to do the comparison)
                    public static bool operator ==({{typeToGenerate.Name}}? left, string? right) => left?._value == right;
                    public static bool operator !=({{typeToGenerate.Name}}? left, string? right) => !(left == right);
                    public static bool operator ==(string? left, {{typeToGenerate.Name}}? right) => left == right?._value;
                    public static bool operator !=(string? left, {{typeToGenerate.Name}}? right) => !(left == right);

                    // null safe conversions
                    public static implicit operator string?({{typeToGenerate.Name}}? s)
                        => s?._value;
                
                    public static implicit operator {{typeToGenerate.Name}}? (string? s)
                         => s is null ? null : new {{typeToGenerate.Name}}(s);

                    // Json serialization support
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