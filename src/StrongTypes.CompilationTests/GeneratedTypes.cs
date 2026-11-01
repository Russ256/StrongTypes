// Compile-only regression coverage: no imports or global usings, and local names
// that would shadow unqualified framework references in generated files.
namespace StrongTypes.CompilationTests;

[global::StrongTypes.StrongType(global::StrongTypes.BaseType.Integer)]
public readonly partial struct IntegerId { }

[global::StrongTypes.StrongType(global::StrongTypes.BaseType.Guid)]
public readonly partial struct GuidId { }

[global::StrongTypes.StrongType(global::StrongTypes.BaseType.Guid)]
public readonly partial record struct GuidRecordId { }

[global::StrongTypes.StrongType(global::StrongTypes.BaseType.Guid)]
public partial class GuidClassId { }

[global::StrongTypes.StrongType(global::StrongTypes.BaseType.Integer)]
public partial class IntegerClassId { }

[global::StrongTypes.StrongType(global::StrongTypes.BaseType.Integer)]
public partial record IntegerRecordId;

[global::StrongTypes.StrongType(global::StrongTypes.BaseType.Guid)]
public sealed partial record class GuidRecordClassId;

internal static class NullableFlowChecks
{
    internal static bool StringIsNull(StringId? value) => value == null || null == value;
    internal static bool StringIsNotNull(StringId? value) => value != null && null != value;
    internal static bool EmailIsNull(EmailId? value) => value == null || null == value;
    internal static bool EmailIsNotNull(EmailId? value) => value != null && null != value;
    internal static bool GuidIsNull(GuidClassId? value) => value == null || null == value;
    internal static bool GuidIsNotNull(GuidClassId? value) => value != null && null != value;
    internal static bool IntegerIsNull(IntegerClassId? value) => value == null || null == value;
    internal static bool IntegerIsNotNull(IntegerClassId? value) => value != null && null != value;
    internal static bool UriIsNull(UriId? value) => value == null || null == value;
    internal static bool UriIsNotNull(UriId? value) => value != null && null != value;

    internal static int ParseGuid(string? value)
    {
        if (GuidClassId.TryParse(value, out GuidClassId? result))
        {
            return result.CompareTo(null);
        }
        return 0;
    }

    internal static int ParseInteger(string? value)
    {
        if (IntegerRecordId.TryParse(value, null, out IntegerRecordId? result))
        {
            return result.Value;
        }
        return 0;
    }
}

[global::StrongTypes.StrongType(global::StrongTypes.BaseType.String, 3)]
public partial class StringId { }

[global::StrongTypes.StrongType(global::StrongTypes.BaseType.EmailAddress)]
public partial class EmailId { }

[global::StrongTypes.StrongType(global::StrongTypes.BaseType.Uri)]
public partial class UriId { }

internal sealed class System { }
internal sealed class Guid { }
internal sealed class Uri { }
internal sealed class Type { }
internal sealed class ArgumentException { }
internal sealed class ArgumentNullException { }
internal sealed class FormatException { }
internal sealed class StringComparer { }
internal sealed class StringComparison { }
internal sealed class IFormatProvider { }
internal sealed class JsonSerializerOptions { }
internal sealed class Utf8JsonReader { }
internal sealed class Utf8JsonWriter { }

// Also shadow the root namespace used for generated exception references.
internal sealed class StrongTypes { }
