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
    [DataRow(null, true)]
    [DataRow("value", false)]
    public void Null_Literal_Equality_Should_Work_In_Both_Directions(string? value, bool expected)
    {
        // Arrange
        TestString? sut = value;

        // Assert
        (sut == null).Should().Be(expected);
        (null == sut).Should().Be(expected);
        (sut != null).Should().Be(!expected);
        (null != sut).Should().Be(!expected);
    }

    [TestMethod]
    public void Assignment_To_String_Should_Succeed()
    {
        // Arrange
        TestString? sut = "test";

        // Act
        string result = sut!;

        // Assert
        result.Should().Be("test");
    }

    [TestMethod]
    public void Constructor_MaxLength_Should_Throw_Exception()
    {
        // Arrange
        string testString = "test";
        Action action = () => _ = new TestStringLength(testString);

        // Act
        MaxLengthException exception = action.Should().Throw<MaxLengthException>()
            .WithMessage($"String of length {testString.Length} exceeds the maximum of 3 for type TestStringLength").Which;

        // Assert
        exception.Type.Should().Be<TestStringLength>();
        exception.MaxLength.Should().Be(3);
        exception.StringLength.Should().Be(testString.Length);
    }

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
        TestString sut = testString!;

        // Assert
        sut.Value.Should().Be(testString);
    }

    [TestMethod]
    [DataRow(null, null, true, DisplayName = "Nulls Equal")]
    [DataRow("s1", "s1", true, DisplayName = "Values Equal")]
    [DataRow("s1", "!s1", false, DisplayName = "Not equal")]
    [DataRow("s1", null, false, DisplayName = "Not equal")]
    [DataRow(null, "s1", false, DisplayName = "Not equal")]
    public void Implicit_Equality(string? left, string? right, bool result)
    {
        // Arrange
        TestString? leftSut = left ?? null!;
        TestString? rithSut = right ?? null!;

        // Act
        bool testResult1 = leftSut! == rithSut!;
        bool testResult2 = leftSut! != rithSut!;

        bool testResult3 = rithSut! == leftSut!;
        bool testResult4 = rithSut! != leftSut!;

        // Assert
        testResult1.Should().Be(result);
        testResult2.Should().Be(!result);
        testResult3.Should().Be(result);
        testResult4.Should().Be(!result);
    }

    [TestMethod]
    [DataRow("s1", "s1", true, DisplayName = "Equal")]
    [DataRow(null, null, true, DisplayName = "Equal")]
    [DataRow("s1", "!s1", false, DisplayName = "Not equal")]
    [DataRow("s1", null, false, DisplayName = "Not equal")]
    [DataRow(null, "s1", false, DisplayName = "Not equal")]
    public void Implicit_String_Equality(string? left, string right, bool result)
    {
        // Arrange
        TestString? leftSut = left ?? null!;

        // Act
        bool testResult1 = leftSut! == right;
        bool testResult2 = leftSut! != right;

        bool testResult3 = right == leftSut!;
        bool testResult4 = right != leftSut!;

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
        TestString sut = testString!;

        // Act
        string result = JsonSerializer.Serialize(sut);

        // Assert
        result.Should().Be($"\"{testString}\"");
    }

    [TestMethod]
    public void Null_To_NullString_Equality()
    {
        // Arrange
        TestString? sut1 = null;
        string? sut2 = null;

        // Act
        bool result = sut1! == sut2!;

        // Asset
        result.Should().BeTrue();
    }

    [TestMethod]
    public void NullString_To_Null_Equality()
    {
        // Arrange
        TestString? sut1 = null;
        string? sut2 = null;

        // Act
        bool result = sut2! == sut1!;

        // Asset
        result.Should().BeTrue();
    }
}
