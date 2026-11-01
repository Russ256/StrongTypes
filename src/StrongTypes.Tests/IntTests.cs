namespace StrongTypes.Tests;

using FluentAssertions;
using System.Globalization;
using System.Text.Json;

[StrongType(BaseType.Integer)]
public readonly partial struct TestInt
{
}

//[StrongType(BaseType.Integer)] // Do not remove Int32!
//public readonly partial struct TestInt32
//{
//}

[TestClass]
public class IntTests
{
    [TestMethod]
    public void Parse_With_Custom_Provider_Should_Use_Its_Negative_Sign()
    {
        // Arrange
        NumberFormatInfo provider = new() { NegativeSign = "minus" };

        // Act
        TestInt result = TestInt.Parse("minus42", provider);

        // Assert
        result.Value.Should().Be(-42);
    }

    [TestMethod]
    public void TryParse_With_Custom_Provider_Should_Use_Its_Negative_Sign()
    {
        // Arrange
        NumberFormatInfo provider = new() { NegativeSign = "minus" };

        // Act
        bool success = TestInt.TryParse("minus42", provider, out TestInt result);

        // Assert
        success.Should().BeTrue();
        result.Value.Should().Be(-42);
    }

    [TestMethod]
    public void TryParse_With_Invariant_Provider_Should_Reject_Custom_Negative_Sign()
    {
        // Act
        bool success = TestInt.TryParse("minus42", CultureInfo.InvariantCulture, out TestInt result);

        // Assert
        success.Should().BeFalse();
        result.Should().Be(default(TestInt));
    }

    [TestMethod]
    public void Constructor_Should_Set_Value()
    {
        // Act
        TestInt sut = new(500);

        // Assert
        sut.ToString().Should().Be("500");
    }

    //[TestMethod]
    //public void Int32_Is_Generated_As_Int()
    //{
    //    // Act
    //    TestInt32 sut = new(500);

    //    // Assert
    //    sut.Value.Should().BeOfType(typeof(int));
    //}

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
