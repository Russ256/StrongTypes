namespace StrongTypes.Tests;

using FluentAssertions;
using System.Text.Json;

[StrongType(typeof(string))]
public partial class TestString
{
}

[TestClass]
public class StringTests
{
    [TestMethod]
    public void Constructor_Should_Set_Value()
    {
        // Arrange
        string testString = "test";

        // Act
        TestString sut = new(testString);

        // Assert
        sut.Value.Should().Be(testString);
    }

    [TestMethod]
    public void Implicit_Constructor_Should_Set_Value()
    {
        // Arrange
        string testString = "test";

        // Act
        TestString sut = testString;

        // Assert
        sut.Value.Should().Be(testString);
    }

    [TestMethod]
    [DataRow("s1", "s1", true, DisplayName = "Equal")]
    [DataRow("s1", "!s1", false, DisplayName = "Not equal")]
    public void Implicit_String_Equality(string left, string right, bool result)
    {
        // Arrange
        string testString = left;
        TestString sut = testString;

        // Act
        bool testResult1 = sut == right;
        bool testResult2 = sut != right;

        bool testResult3 = right == sut;
        bool testResult4 = right != sut;

        // Assert
        testResult1.Should().Be(result);
        testResult2.Should().Be(!result);
        testResult3.Should().Be(result);
        testResult4.Should().Be(!result);
    }

    //[TestMethod]
    //public void Json_Deserialize_Should_Succeed()
    //{
    //    // Arrange
    //    int value = 500;
    //    string json = value.ToString();

    //    // Act
    //    TestInt result = JsonSerializer.Deserialize<TestInt>(json);

    //    // Assert
    //    result.Value.Should().Be(value);
    //}

    //[TestMethod]
    //public void Json_Serialize_Should_Not_Include_Quotes()
    //{
    //    // Arrange
    //    int value = 500;
    //    TestInt sut = new(value);

    //    // Act
    //    string result = JsonSerializer.Serialize(sut);

    //    // Assert
    //    result.Should().Be(value.ToString());
    //}

    //[TestMethod]
    //[DataRow("", DisplayName = "Empty")]
    //[DataRow("1,000", DisplayName = "1,000")]
    //[DataRow("0.1", DisplayName = "0.1")]
    //public void String_Constructor_Should_Fail(string value)
    //{
    //    // Act
    //    Action action = () => TestInt.Parse(value);

    //    // Assert
    //    action.Should().Throw<FormatException>().WithMessage($"'{value}' is not a valid TestInt.");
    //}
}
