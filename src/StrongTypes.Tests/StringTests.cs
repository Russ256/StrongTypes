namespace StrongTypes.Tests;

using FluentAssertions;
using System.Text.Json;

[StrongType(BaseType.String)]
public partial class TestString
{
}

[StrongType(BaseType.String, 3)]
public partial class TestStringLength
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
    public void Constructor_MaxLength_Should_Throw_Exception()
    {
        // Arrange
        string testString = "test";
        Action action = () => _ = new TestStringLength(testString);

        // Act
        action.Should().Throw<MaxLengthException>().WithMessage($"String of length {testString.Length} exceeds the maximum of 3 for type TestStringLength");
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
    public void Implicit_Equality(string left, string right, bool result)
    {
        // Arrange
        TestString sut1 = left;
        TestString sut2 = right;

        // Act
        bool testResult1 = sut1 == sut2;
        bool testResult2 = sut1 != sut2;

        bool testResult3 = sut2 == sut1;
        bool testResult4 = sut2 != sut1;

        // Assert
        testResult1.Should().Be(result);
        testResult2.Should().Be(!result);
        testResult3.Should().Be(result);
        testResult4.Should().Be(!result);
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

    [TestMethod]
    public void Json_Deserialize_Should_Succeed()
    {
        // Arrange
        string testString = "test";
        string json = $"\"{testString}\"";

        // Act
        TestString result = JsonSerializer.Deserialize<TestString>(json)!;

        // Assert
        result.Should().NotBeNull();
        result.ToString().Should().Be(testString);
    }

    [TestMethod]
    public void Json_Serialize_Should_Include_Quotes()
    {
        // Arrange
        string testString = "test";
        TestString sut = testString;

        // Act
        string result = JsonSerializer.Serialize(sut);

        // Assert
        result.Should().Be($"\"{testString}\"");
    }
}