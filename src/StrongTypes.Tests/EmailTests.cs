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
    public void Constructor_Invalid_Email_Should_Throw_Exception()
    {
        // Arrange
        string testString = "test";
        Action action = () => _ = new TestEmail(testString);

        // Act
        action.Should().Throw<EmailAddressFormatException>();
    }
}