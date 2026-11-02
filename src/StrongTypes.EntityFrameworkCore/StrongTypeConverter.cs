namespace StrongTypes.EntityFrameworkCore;

using System;
using System.Linq.Expressions;
using System.Reflection;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

/// <summary>Converts a generated strong type using its Value property and validating constructor.</summary>
public sealed class StrongTypeConverter<TStrong, TValue> : ValueConverter<TStrong, TValue>
{
    public StrongTypeConverter() : base(CreateToProvider(), CreateFromProvider())
    {
    }

    private static Expression<Func<TStrong, TValue>> CreateToProvider()
    {
        ParameterExpression parameter = Expression.Parameter(typeof(TStrong), "value");
        Expression value = Expression.Property(parameter, "Value");
        if (value.Type == typeof(Uri) && typeof(TValue) == typeof(string))
        {
            value = Expression.Property(value, nameof(Uri.OriginalString));
        }

        return Expression.Lambda<Func<TStrong, TValue>>(value, parameter);
    }

    private static Expression<Func<TValue, TStrong>> CreateFromProvider()
    {
        ParameterExpression parameter = Expression.Parameter(typeof(TValue), "value");
        ConstructorInfo constructor = typeof(TStrong).GetConstructor([typeof(TValue)])
            ?? throw new InvalidOperationException($"{typeof(TStrong)} requires a constructor accepting {typeof(TValue)}.");

        return Expression.Lambda<Func<TValue, TStrong>>(Expression.New(constructor, parameter), parameter);
    }
}
