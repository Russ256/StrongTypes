namespace StrongTypes.Tests;

using FluentAssertions;

[TestClass]
public class StandardExceptionTests
{
    [TestMethod]
    public void Length_Exceptions_From_Another_Assembly_Should_Use_Standard_ArgumentException()
    {
        // Arrange
        Action action = () => _ = new global::StrongTypes.CompilationTests.StringId("abcd");

        // Act / Assert
        action.Should().ThrowExactly<ArgumentException>()
            .WithMessage("String of length 4 exceeds the maximum of 3 for type StringId*")
            .Which.ParamName.Should().Be("value");
    }

    [TestMethod]
    [DataRow("")]
    [DataRow("invalid")]
    public void Email_Exceptions_From_Another_Assembly_Should_Use_Standard_FormatException(string value)
    {
        // Arrange
        Action action = () => _ = new global::StrongTypes.CompilationTests.EmailId(value);

        // Act / Assert
        action.Should().ThrowExactly<FormatException>().WithMessage("Value is not a valid email address.");
    }
}
