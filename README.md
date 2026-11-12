# StrongTypes

A C# source generator for strongly typed GUIDs, integers, strings, email addresses, and URIs.
Use types such as `CustomerId` and `OrderId` in your APIs instead of passing raw
GUIDs or strings everywhere. The compiler can then catch accidental mix-ups
between values that represent different things.

StrongTypes generates the constructors, value access, equality, and JSON
conversion code for you. You can use it on its own; Entity Framework Core
support is an optional addition.

## Getting started

Install the generator in the project where you declare your strong types:

```powershell
dotnet add package StrongTypes
```

Declare a partial type in a namespace and choose the value it wraps:

```csharp
using StrongTypes;

namespace Example;

[StrongType(BaseType.Guid)]
public readonly partial struct CustomerId { }

[StrongType(BaseType.Guid)]
public readonly partial struct OrderId { }

public static class Orders
{
    public static string Describe(CustomerId customerId, OrderId orderId)
    {
        return $"Customer {customerId.Value} placed order {orderId.Value}.";
    }
}
```

Build the project to generate the implementation. You can then create and use
these types like any other C# type:

```csharp
CustomerId customerId = CustomerId.New();
OrderId orderId = new OrderId(Guid.NewGuid());

string description = Orders.Describe(customerId, orderId);
Guid rawCustomerId = customerId.Value;

// Orders.Describe(orderId, customerId); // Compile error: the types do not match.
```

The usage examples assume `using System;` and `using Example;` where needed.
No database or runtime registration is required for standalone use.

## Supported types

Every generated type exposes its underlying value through a read-only `Value`
property and provides `ToString()`, equality, and a System.Text.Json converter.

| Attribute | Wrapped value | Supported declarations | Additional features |
| --- | --- | --- | --- |
| `BaseType.Guid` | `System.Guid` | Class, struct, record class, record struct | GUID/string constructors, `New()`, `Empty`, `Parse`, `TryParse` |
| `BaseType.Integer` | `int` | Class, struct, record class, record struct | Integer/string constructors, `Parse`, `TryParse` |
| `BaseType.String` | `string` | Ordinary class | Optional maximum length, implicit string conversions |
| `BaseType.EmailAddress` | `string` | Ordinary class | Email validation, maximum length, implicit string conversions |
| `BaseType.Uri` | `System.Uri` | Ordinary class | URI/string constructors, relative or absolute URIs |

All declarations must be partial. See [declaration requirements](#declaration-requirements)
for restrictions and details about record equality.

### Strings, email addresses, and URIs

```csharp
using StrongTypes;

namespace Example;

[StrongType(BaseType.String, 100)]
public partial class CustomerName { }

[StrongType(BaseType.EmailAddress)]
public partial class EmailAddress { }

[StrongType(BaseType.Uri)]
public partial class Website { }
```

Create these values through the generated constructors:

```csharp
CustomerName name = new CustomerName("Alex Smith");
EmailAddress email = new EmailAddress("alex@example.com");
Website website = new Website("https://example.com");

string text = name.Value;
Uri uri = website.Value;
```

String types have no length limit unless you supply one. Email types validate
the address and default to a maximum of 254 UTF-16 code units. Constructors
reject null values, invalid emails, and values exceeding the configured limit;
see [validation exceptions](#validation-exceptions) for the exact exceptions.

### Parsing and comparison

GUID and integer types provide `Parse` and `TryParse` methods:

```csharp
CustomerId customerId = CustomerId.Parse("3289446b-e0d8-485e-a5a6-0e2a2b986dee");
bool parsed = CustomerId.TryParse("not-a-guid", out CustomerId result);
```

Classes and structs wrapping GUIDs or integers also implement `IComparable<T>`
for sorting. Their equality uses the wrapped value. Records use the compiler's
record equality and do not receive generated comparison methods.

### JSON serialization

Generated types work with `System.Text.Json` without registering converters.
They serialize as the wrapped value rather than as an object containing `Value`:

```csharp
using System.Text.Json;

CustomerId customerId = new CustomerId("3289446b-e0d8-485e-a5a6-0e2a2b986dee");
string json = JsonSerializer.Serialize(customerId);
// "3289446b-e0d8-485e-a5a6-0e2a2b986dee" (a JSON string)

CustomerId restored = JsonSerializer.Deserialize<CustomerId>(json);
```

Integer types serialize as JSON numbers; GUID, string, email, and URI types
serialize as JSON strings. Deserialization applies constructor validation.

## Entity Framework Core (optional)

The optional `StrongTypes.EntityFrameworkCore` package supports EF Core 10 on
.NET 10. Reference it from your persistence project alongside your EF Core
database provider. Continue referencing the `StrongTypes` generator from the
project declaring your strong types, and rebuild that project with this version
of the generator so it emits the runtime discovery metadata.

Install the integration in your persistence project:

```powershell
dotnet add package StrongTypes.EntityFrameworkCore
```

Register the assemblies containing your strong types once in your context:

```csharp
using Microsoft.EntityFrameworkCore;
using StrongTypes.EntityFrameworkCore;

namespace Example;

[StrongTypes.StrongType(StrongTypes.BaseType.Guid)]
public readonly partial struct CustomerId { }

public class AppDbContext : DbContext
{
    protected override void ConfigureConventions(
        ModelConfigurationBuilder configurationBuilder)
    {
        base.ConfigureConventions(configurationBuilder);
        configurationBuilder.AddStrongTypes(typeof(CustomerId).Assembly);
    }
}
```

Pass additional assemblies to the same call when types live in multiple projects.
All generated types in those assemblies, including internal types, are registered
before EF discovers properties. No per-type converter classes or per-property
registration are required. GUID and integer wrappers use their underlying CLR
types; string and email wrappers use strings. URI wrappers use strings and
preserve `Uri.OriginalString`, including relative URIs.

Nullable properties retain database nulls. Reading non-null values uses the
generated constructor, enforcing the same validation as application code.
Queries comparing a property to a strong-type value use the converter; accessing
members such as `.Value` inside a database query is subject to EF Core's value
conversion translation limitations. Only the wrapped value is persisted; any
additional user-defined state on a strong type is not stored by this integration.
Explicit property configuration in `OnModelCreating` can override the converter.

The integration discovers retained metadata emitted by the generator; it does
not depend on retaining `StrongTypeAttribute`, and the core generator has no EF
Core dependency. Assembly scanning and runtime converter construction require
reflection and dynamic code; this registration API is not intended for trimmed
or Native AOT applications.

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
Required properties and fields are rejected with `ST0017`, including members on
other partial declarations or inherited from base types. Generated parsing and
JSON conversion cannot initialize this additional state. Remove `required` or
put the additional state in a separate model.
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

## Continuous integration

GitHub Actions runs Release builds and tests on Windows and Linux for pushes
and pull requests, and supports manual runs. Solution builds include the
compile-only regression checks. TRX test reports are retained as workflow
artifacts for 14 days.
