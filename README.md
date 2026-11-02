# StrongTypes

A C# source generator for strongly typed GUIDs, integers, strings, email addresses, and URIs.

## Development

Install the .NET 10 SDK. `global.json` selects an installed stable .NET 10 SDK.
The generator targets .NET Standard 2.0 for analyzer compatibility; its tests
compile and exercise generated types on .NET 10.

Run from the repository root:

```powershell
dotnet restore src/StrongTypes.slnx
dotnet build src/StrongTypes.slnx --no-restore
dotnet test src/StrongTypes.slnx --no-restore
```

Solution builds also compile `src/StrongTypes.CompilationTests/`, which checks
generated code with implicit usings disabled and conflicting local type names.

## Declaration requirements

Strong types must be partial, non-generic types declared directly in a namespace.
The generator reports errors for global-namespace (`ST0005`), nested (`ST0006`),
and generic (`ST0007`) declarations. String and email types must be ordinary
classes; structs and records receive `ST0008`. URI types also require ordinary
classes (`ST0002`). File-local types are rejected with `ST0009`; remove the
`file` modifier to allow generation in a separate source file.
GUID and integer classes and structs implement `IEquatable<T>` and
`IComparable<T>` using the wrapped value. Reference types compare greater than
`null`. Records retain compiler-generated equality, including additional state
and record inheritance, and no longer receive generated `IComparable<T>` or
`CompareTo` members. Use an explicit comparer for sorting records or using them
in sorted collections; a value-only comparer intentionally groups records with
the same wrapped value even when their other state differs.

Primary constructors, including positional record declarations and empty
parameter lists, are rejected with `ST0010`. Remove the primary-constructor
parameter list and use the generated value constructors instead.
Static classes (`ST0011`), abstract classes or records (`ST0012`), and ref structs
(`ST0013`) are unsupported. Use a concrete instance class, ordinary struct, or
supported record type instead. These checks include modifiers on other partial
declarations of the same type.
Type names that conflict with generated public members (for example, `Value`,
or `Parse` for integer/GUID types) are rejected with `ST0014`. Rename the type
to resolve the conflict. Generated backing fields and JSON converter helpers
automatically choose an unused name, so types named `JsonConverter` are supported.

String and email equality operators compare instances of the same generated type
and support null literals on either side. Comparisons with raw strings use the
implicit conversion to the strong type, including its constructor validation.
Compare `.Value` directly when checking unvalidated text without creating a wrapper.

## Validation exceptions

String limits are optional: `[StrongType(BaseType.String)]` has no length limit,
while `[StrongType(BaseType.String, 0)]` permits only the empty string.
Email types default to a limit of 254; an explicit limit from 0 through 254 is
honored in addition to email validation. A zero email limit permits no valid values.
Negative limits produce `ST0004`, email limits above 254 produce `ST0016`, and
supplying a limit for integer, GUID, or URI types produces `ST0015`.
Lengths count UTF-16 code units (`string.Length`), so many emoji count as two.
Constructor calls, implicit conversions, and JSON deserialization enforce the same limits.

Length violations throw `System.ArgumentException` with `ParamName` set to
`value` and the actual length, maximum length, and type name in the message.
Invalid emails throw `System.FormatException`; null constructor arguments still
throw `System.ArgumentNullException`.

During JSON deserialization, string/email, GUID, and URI constructor validation
failures are wrapped in `System.Text.Json.JsonException`, with the original
exception preserved as `InnerException` and the JSON path supplied by the serializer.

These standard exceptions replace the generated `MaxLengthException` and
`EmailAddressFormatException`. Update existing catch clauses accordingly.
No public exception types are injected into consuming assemblies, so callers
can catch the same standard types across library boundaries.

## Continuous integration

GitHub Actions runs Release builds and tests on Windows and Linux for pushes
and pull requests, and supports manual runs. Solution builds include the
compile-only regression checks. TRX test reports are retained as workflow
artifacts for 14 days.
