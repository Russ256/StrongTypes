namespace StrongTypes;

using Microsoft.CodeAnalysis;

internal readonly struct TypeAnalysis
{
    internal TypeAnalysis(TypeToGenerate? type, Diagnostic diagnostic)
    {
        Type = type;
        Diagnostic = diagnostic;
    }

    internal TypeToGenerate? Type { get; }
    internal Diagnostic Diagnostic { get; }
}
