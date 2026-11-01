// Compile-only regression coverage: no imports or global usings, and local names
// that would shadow unqualified framework references in generated files.
namespace StrongTypes.CompilationTests;

[global::StrongTypes.StrongType(global::StrongTypes.BaseType.Integer)]
public readonly partial struct IntegerId { }

[global::StrongTypes.StrongType(global::StrongTypes.BaseType.Guid)]
public readonly partial struct GuidId { }

[global::StrongTypes.StrongType(global::StrongTypes.BaseType.Guid)]
public readonly partial record struct GuidRecordId { }

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
