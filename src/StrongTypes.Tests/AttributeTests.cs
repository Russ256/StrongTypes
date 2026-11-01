namespace StrongTypes.Tests;

using FluentAssertions;
using Kind = global::StrongTypes.BaseType;
using Strong = global::StrongTypes.StrongTypeAttribute;

[global::StrongTypes.StrongTypeAttribute(global::StrongTypes.BaseType.Integer)]
public readonly partial struct QualifiedInteger
{
}

[Strong(Kind.Guid)]
public readonly partial struct AliasedGuid
{
}

internal static class StringLimits
{
    internal const int Length = 2;
}

[Strong(maxLength: StringLimits.Length + 1, baseType: Kind.String)]
public partial class ConstantLengthString
{
}

[TestClass]
public class AttributeTests
{
    [TestMethod]
    public void Qualified_Attribute_And_BaseType_Should_Generate_Integer()
    {
        // Act
        QualifiedInteger sut = new(42);

        // Assert
        sut.Value.Should().Be(42);
    }

    [TestMethod]
    public void Aliased_Attribute_And_BaseType_Should_Generate_Guid()
    {
        // Arrange
        Guid value = Guid.NewGuid();

        // Act
        AliasedGuid sut = new(value);

        // Assert
        sut.Value.Should().Be(value);
    }

    [TestMethod]
    public void Named_Arguments_With_Constant_Expression_Should_Allow_Maximum_Length()
    {
        // Act
        ConstantLengthString sut = new("abc");

        // Assert
        sut.Value.Should().Be("abc");
    }

    [TestMethod]
    public void Named_Arguments_With_Constant_Expression_Should_Enforce_Maximum_Length()
    {
        // Arrange
        Action action = () => _ = new ConstantLengthString("abcd");

        // Act / Assert
        action.Should().ThrowExactly<ArgumentException>()
            .WithMessage("String of length 4 exceeds the maximum of 3 for type ConstantLengthString*")
            .Which.ParamName.Should().Be("value");
    }
}
