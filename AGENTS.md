# Repository Guidelines

## Project Structure & Module Organization

`src/StrongTypes.sln` contains two C# projects:

- `src/StrongTypes/`: a Roslyn incremental source generator targeting `netstandard2.0`. `Generator.cs` discovers annotated types; `Attributes.cs` and `Exceptions.cs` supply generated support code. `SourceGenerators/` contains shared generation logic and implementations for GUID, integer, string/email, and URI types.
- `src/StrongTypes.Tests/`: a `net9.0` test executable with behavior tests grouped by base type, such as `GuidTests.cs` and `EmailTests.cs`. It references the generator as an analyzer so test fixture types exercise generated code.

## Build, Test, and Development Commands

Run these commands from the repository root with a .NET SDK capable of targeting .NET 9:

- `dotnet restore src/StrongTypes.sln`: restore NuGet dependencies.
- `dotnet build src/StrongTypes.sln --no-restore`: compile the generator and its consuming tests. The generator project also creates a NuGet package on build.
- `dotnet test src/StrongTypes.sln --no-restore`: run the MSTest suite.
- `dotnet run --project src/StrongTypes.Tests/StrongTypes.Tests.csproj`: run the test executable directly through its MSTest runner.

This repository is a generator library; local development centers on building and testing rather than starting an application.

## Coding Style & Naming Conventions

Follow existing C# style: four-space indentation, braces on separate lines, file-scoped namespaces, PascalCase types and members, camelCase parameters and locals, and underscore-prefixed private fields. Prefer explicit local types where existing code does. Keep emitted C# templates consistent with surrounding raw string literals. Edit generator sources rather than generated `.g.cs` output.

Roslyn analyzer rules are enabled, including extended analyzer rules. No repository-wide formatter configuration is checked in.

## Testing Guidelines

Use MSTest attributes and FluentAssertions. Name tests descriptively, following patterns such as `CompareTo_When_LessThan_Returns_Negative`, and organize bodies with Arrange/Act/Assert comments. Add regression tests to the relevant base-type test file, covering generated behavior and affected declaration forms. Tests run in parallel at method scope; avoid shared mutable state. Coverage tooling is referenced, but no minimum coverage threshold is configured.

## Commit & Pull Request Guidelines

History uses short, descriptive subjects such as `Added EmailAddress support (#12)`; no strict Conventional Commits format is established. Keep commits focused. In pull requests, explain the behavior change, link related issues when applicable, and report build/test results. Include a small annotated-type example when changing generated APIs or supported declarations.
