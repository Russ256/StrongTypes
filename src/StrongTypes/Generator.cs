namespace StrongTypes;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using StrongTypes.SourceGenerators;
using System.Collections.Generic;
using System.Linq;
using System.Text;

[Generator(LanguageNames.CSharp)]
public class Generator : IIncrementalGenerator
{
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        // Add our attribute to the project
        context.RegisterPostInitializationOutput(ctx => ctx.AddSource(
                                                        "StrongTypes.Attributes.g.cs",
                                                        SourceText.From(Attributes.Source, Encoding.UTF8)));

        IncrementalValuesProvider<TypeAnalysis> analyses = context.SyntaxProvider
            .ForAttributeWithMetadataName(
                "StrongTypes.StrongTypeAttribute",
                predicate: static (node, _) => node is TypeDeclarationSyntax,
                transform: static (attributeContext, ct) => AnalyzeType(attributeContext, ct));

        IncrementalValuesProvider<TypeToGenerate> types = analyses
            .Where(static analysis => analysis.Type.HasValue)
            .Select(static (analysis, _) => analysis.Type.GetValueOrDefault())
            .WithTrackingName("StrongTypeModels");

        IncrementalValuesProvider<(string HintName, string Source)> sources = types
            .Select(static (type, _) => (type.HintName, SourceGenerator.Generate(type)))
            .WithTrackingName("StrongTypeSources");
        context.RegisterSourceOutput(sources, static (spc, source) =>
            spc.AddSource(source.HintName, source.Source));
        context.RegisterSourceOutput(analyses, static (spc, analysis) =>
        {
            if (analysis.Diagnostic is Diagnostic diagnostic)
            {
                spc.ReportDiagnostic(diagnostic);
            }
        });
    }

    private static TypeAnalysis AnalyzeType(GeneratorAttributeSyntaxContext context, System.Threading.CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        TypeDeclarationSyntax declaration = (TypeDeclarationSyntax)context.TargetNode;
        INamedTypeSymbol symbol = (INamedTypeSymbol)context.TargetSymbol;
        AttributeData attribute = context.Attributes[0];
        if (attribute == null || attribute.ConstructorArguments.Length == 0
            || attribute.ConstructorArguments.Any(argument => argument.Kind == TypedConstantKind.Error))
        {
            // Invalid expressions already receive compiler diagnostics.
            return default;
        }

        Location location = attribute.ApplicationSyntaxReference?.GetSyntax(ct).GetLocation()
            ?? declaration.GetLocation();
        if (!declaration.Modifiers.Any(modifier => modifier.Text == "partial"))
        {
            return new TypeAnalysis(null, CreateDiagnostic("ST0003", "Types must be defined partial", location));
        }

        if (symbol.IsFileLocal)
        {
            return new TypeAnalysis(null, CreateDiagnostic("ST0009", "File-local strong types are not supported; remove the file modifier", location));
        }

        if (symbol.IsStatic)
        {
            return new TypeAnalysis(null, CreateDiagnostic("ST0011", "Static strong types are not supported; remove the static modifier", location));
        }
        if (symbol.IsAbstract)
        {
            return new TypeAnalysis(null, CreateDiagnostic("ST0012", "Abstract strong types are not supported; remove the abstract modifier", location));
        }
        if (symbol.IsRefLikeType)
        {
            return new TypeAnalysis(null, CreateDiagnostic("ST0013", "Ref struct strong types are not supported; remove the ref modifier", location));
        }

        if (symbol.DeclaringSyntaxReferences.Any(reference =>
            reference.GetSyntax(ct) is TypeDeclarationSyntax type && type.ParameterList != null))
        {
            return new TypeAnalysis(null, CreateDiagnostic("ST0010", "Primary constructors are not supported for strong types; remove the parameter list", location));
        }

        if (symbol.ContainingNamespace.IsGlobalNamespace)
        {
            return new TypeAnalysis(null, CreateDiagnostic("ST0005", "Strong types must be declared in a namespace", location));
        }
        if (symbol.ContainingType != null)
        {
            return new TypeAnalysis(null, CreateDiagnostic("ST0006", "Nested strong types are not supported; declare the type at namespace scope", location));
        }
        if (symbol.Arity > 0)
        {
            return new TypeAnalysis(null, CreateDiagnostic("ST0007", "Generic strong types are not supported", location));
        }

        if (HasRequiredMembers(symbol, ct))
        {
            return new TypeAnalysis(null, CreateDiagnostic("ST0017",
                "Required members are not supported for strong types, including inherited members; remove the required modifier or use a separate model", location));
        }

        object? baseType = attribute.ConstructorArguments[0].Value;
        int? maxLength = attribute.ConstructorArguments.Length > 1
            && attribute.ConstructorArguments[1].Value is int length
            ? length : null;
        if (maxLength.HasValue)
        {
            if (baseType is not (3 or 4))
            {
                return new TypeAnalysis(null, CreateDiagnostic("ST0015", "Maximum length is only supported for string and email strong types", location));
            }
            if (maxLength < 0)
            {
                return new TypeAnalysis(null, CreateDiagnostic("ST0004", "Maximum length must be non-negative", location));
            }
            if (baseType is 4 && maxLength > 254)
            {
                return new TypeAnalysis(null, CreateDiagnostic("ST0016", "Email maximum length must not exceed 254", location));
            }
        }
        if ((baseType is 3 || baseType is 4) && declaration is not ClassDeclarationSyntax)
        {
            return new TypeAnalysis(null, CreateDiagnostic("ST0008", "String and email strong types must be declared as classes, not structs or records", location));
        }

        TypeToGenerate typeToGenerate = new()
        {
            Name = declaration.Identifier.Text,
            Modifiers = declaration.Modifiers.ToString(),
            ClassOrStruct = declaration is RecordDeclarationSyntax record ? record.ClassOrStructKeyword.Text : declaration.Keyword.Text,
            IsClass = declaration is ClassDeclarationSyntax,
            IsRecord = declaration is RecordDeclarationSyntax,
            IsReferenceType = symbol.IsReferenceType,
            Namespace = symbol.ContainingNamespace.ToDisplayString(),
            HintName = GetHintName(symbol),
            ValueFieldName = GetAvailableMemberName(symbol, "_value"),
            ConverterName = GetAvailableMemberName(symbol, "JsonConverter"),
        };

        // Roslyn resolves aliases, constant expressions, and named argument order.
        switch (attribute.ConstructorArguments[0].Value)
        {
            case 0: // BaseType.Integer
                typeToGenerate.BaseTypeName = "int";
                break;
            case 1: // BaseType.Guid
                typeToGenerate.BaseTypeName = "global::System.Guid";
                break;
            case 2: // BaseType.Uri
                if (declaration is not ClassDeclarationSyntax)
                {
                    return new TypeAnalysis(null, CreateDiagnostic("ST0002", "Only class supported for Uri type", location));
                }
                typeToGenerate.BaseTypeName = "global::System.Uri";
                break;
            case 3: // BaseType.String
                typeToGenerate.BaseTypeName = "string";
                typeToGenerate.MaxLength = maxLength;
                break;
            case 4: // BaseType.EmailAddress
                typeToGenerate.BaseTypeName = "string";
                typeToGenerate.IsEmail = true;
                typeToGenerate.MaxLength = maxLength ?? 254;
                break;
            default:
                return new TypeAnalysis(null, CreateDiagnostic("ST0001", "Base type is not supported", location));
        }
        if (ConflictsWithPublicMemberName(symbol.Name, typeToGenerate))
        {
            return new TypeAnalysis(null, CreateDiagnostic("ST0014",
                $"Strong type name '{symbol.Name}' conflicts with a generated public member; rename the type", location));
        }

        return new TypeAnalysis(typeToGenerate, null);
    }

    private static bool HasRequiredMembers(INamedTypeSymbol symbol, System.Threading.CancellationToken ct)
    {
        for (INamedTypeSymbol? type = symbol; type != null; type = type.BaseType)
        {
            ct.ThrowIfCancellationRequested();
            if (type.GetMembers().Any(member => member is IPropertySymbol { IsRequired: true }
                or IFieldSymbol { IsRequired: true }))
            {
                return true;
            }
        }

        return false;
    }

    private static bool ConflictsWithPublicMemberName(string name, TypeToGenerate type)
    {
        if (name is "Value" or "ToString" or "Equals" or "GetHashCode")
        {
            return true;
        }
        if (type.BaseTypeName is "int" or "global::System.Guid"
            && (name is "Parse" or "TryParse" || (name == "CompareTo" && !type.IsRecord)))
        {
            return true;
        }
        return type.BaseTypeName == "global::System.Guid" && name is "New" or "Empty";
    }

    private static string GetAvailableMemberName(INamedTypeSymbol symbol, string preferredName)
    {
        string name = preferredName;
        int suffix = 0;
        while (name == symbol.Name || symbol.GetMembers(name).Length > 0)
        {
            name = preferredName + (++suffix).ToString(System.Globalization.CultureInfo.InvariantCulture);
        }
        return name;
    }

    private static string GetHintName(INamedTypeSymbol symbol)
    {
        // Metadata names preserve generic arity; '+' distinguishes containing types
        // from namespaces without introducing filesystem path separators.
        Stack<string> typeNames = new();
        for (INamedTypeSymbol? type = symbol; type != null; type = type.ContainingType)
        {
            typeNames.Push(type.MetadataName);
        }

        string prefix = symbol.ContainingNamespace.IsGlobalNamespace
            ? string.Empty : symbol.ContainingNamespace.ToDisplayString() + ".";
        return prefix + string.Join("+", typeNames) + ".g.cs";
    }

    private static Diagnostic CreateDiagnostic(string id, string message, Location location)
    {
        return Diagnostic.Create(
            new DiagnosticDescriptor(id, "Not Supported", message, "Strong Types", DiagnosticSeverity.Error, true),
            location);
    }
}
