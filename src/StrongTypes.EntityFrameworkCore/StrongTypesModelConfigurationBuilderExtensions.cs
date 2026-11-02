namespace StrongTypes.EntityFrameworkCore;

using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using Microsoft.EntityFrameworkCore;

/// <summary>Registers generated strong types before EF Core discovers entity properties.</summary>
public static class StrongTypesModelConfigurationBuilderExtensions
{
    /// <summary>
    /// Registers converters for strong types generated in the supplied assemblies.
    /// URI values are stored as strings; other types use their underlying CLR type.
    /// </summary>
    [RequiresUnreferencedCode("Assembly scanning requires generated strong types and their constructors and Value properties to be preserved.")]
    [RequiresDynamicCode("Converter types and expression trees are constructed at runtime.")]
    public static ModelConfigurationBuilder AddStrongTypes(
        this ModelConfigurationBuilder configurationBuilder, params Assembly[] assemblies)
    {
        ArgumentNullException.ThrowIfNull(configurationBuilder);
        ArgumentNullException.ThrowIfNull(assemblies);

        foreach (Assembly assembly in assemblies.Distinct())
        {
            ArgumentNullException.ThrowIfNull(assembly);
            foreach (Type type in assembly.GetTypes())
            {
                CustomAttributeData? metadata = type.CustomAttributes.SingleOrDefault(attribute =>
                    attribute.AttributeType.FullName == "StrongTypes.StrongTypeMetadataAttribute");
                if (metadata is null)
                {
                    continue;
                }

                Type valueType = (Type)metadata.ConstructorArguments[0].Value!;
                Type providerType = valueType == typeof(Uri) ? typeof(string) : valueType;
                Type converterType = typeof(StrongTypeConverter<,>).MakeGenericType(type, providerType);
                configurationBuilder.Properties(type).HaveConversion(converterType);
            }
        }

        return configurationBuilder;
    }
}
