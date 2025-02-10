namespace StrongTypes.Tests;

using FluentAssertions;
using System.Diagnostics;
using System.Text.Json;

[StrongType(typeof(int))]
public readonly partial struct TestInt
{
}

[TestClass]
public class IntTests
{
    [TestMethod]
    public void Constructor_Should_Set_Value()
    {
        // Act
        TestInt sut = new(500);

        // Assert
        sut.ToString().Should().Be("500");
    }

    [TestMethod]
    public void Int32_Is_Generated_As_Int()
    {
        // Act
        TestInt32 sut = new(500);

        // Assert
        sut.Value.Should().BeOfType(typeof(int));
    }

    [TestMethod]
    public void Json_Deserialize_Should_Succeed()
    {
        // Arrange
        int value = 500;
        string json = value.ToString();

        // Act
        TestInt result = JsonSerializer.Deserialize<TestInt>(json);

        // Assert
        result.Value.Should().Be(value);
    }

    [TestMethod]
    public void Json_Serialize_Should_Not_Include_Quotes()
    {
        // Arrange
        int value = 500;
        TestInt sut = new(value);

        // Act
        string result = JsonSerializer.Serialize(sut);

        // Assert
        result.Should().Be(value.ToString());
    }

    [TestMethod]
    [DataRow("100", 100, DisplayName = "100")]
    public void String_Constructor_Should_Equal_Value(string value, int result)
    {
        // Act
        TestInt sut = new(value);

        // Assert
        sut.Value.Should().Be(result);
    }

    [TestMethod]
    [DataRow("", DisplayName = "Empty")]
    [DataRow("1,000", DisplayName = "1,000")]
    [DataRow("0.1", DisplayName = "0.1")]
    public void String_Constructor_Should_Fail(string value)
    {
        // Act
        Action action = () => TestInt.Parse(value);

        // Assert
        action.Should().Throw<FormatException>().WithMessage($"'{value}' is not a valid TestInt.");
    }
}

[StrongType(typeof(Int32))] // Do not remove Int32!
public readonly partial struct TestInt32
{
}