namespace StrongTypes.Tests;

using FluentAssertions;

[StrongType(BaseType.EmailAddress)]
public partial class TestEmail
{
}

[TestClass]
public class EmailTests
{
    [TestMethod]
    public void Constructor_Should_Set_Value()
    {
        // Arrange
        string testString = "test@data.com";

        // Act
        TestEmail sut = new(testString);

        // Assert
        sut.Value.Should().Be(testString);
    }

    [TestMethod]
    public void Implicit_Constructor_Should_Set_Value()
    {
        // Arrange
        string testString = "test@data.com";

        // Act
        TestEmail sut = testString!;

        // Assert
        sut.Value.Should().Be(testString);
    }

    [TestMethod]
    [DataRow("test")]
    [DataRow("")]
    [DataRow(" ")]
    public void Constructor_Invalid_Email_Should_Throw_Exception(string value)
    {
        // Arrange
        Action action = () => _ = new TestEmail(value);

        // Act
        action.Should().ThrowExactly<FormatException>().WithMessage("Value is not a valid email address.");
    }

    [TestMethod]
    public void Implicit_Conversion_Of_Empty_Email_Should_Throw_Format_Exception()
    {
        // Arrange
        Action action = () => { TestEmail? email = string.Empty; };

        // Act / Assert
        action.Should().ThrowExactly<FormatException>().WithMessage("Value is not a valid email address.");
    }

    [TestMethod]
    public void Constructor_Null_Email_Should_Throw_ArgumentNullException()
    {
        // Arrange
        Action action = () => _ = new TestEmail(null!);

        // Act / Assert
        action.Should().Throw<ArgumentNullException>();
    }
}
