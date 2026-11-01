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
