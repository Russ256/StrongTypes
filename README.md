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

## Continuous integration

GitHub Actions runs Release builds and tests on Windows and Linux for pushes
and pull requests, and supports manual runs. Solution builds include the
compile-only regression checks. TRX test reports are retained as workflow
artifacts for 14 days.
