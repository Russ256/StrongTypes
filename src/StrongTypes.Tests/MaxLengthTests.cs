namespace StrongTypes.Tests;

using FluentAssertions;
using System.Text.Json;

[StrongType(BaseType.String, 0)]
public partial class EmptyString { }

[StrongType(BaseType.String, 1)]
public partial class SingleCodeUnitString { }

[StrongType(BaseType.EmailAddress, 6)]
public partial class ShortEmail { }

[StrongType(BaseType.EmailAddress, 0)]
public partial class ZeroLengthEmail { }

[TestClass]
public class MaxLengthTests
{
    [TestMethod]
    public void Explicit_Zero_Should_Allow_Only_Empty_Strings()
    {
        // Arrange
        Action constructor = () => _ = new EmptyString("a");
        Action conversion = () => { EmptyString? value = "a"; };
        Action deserialize = () => JsonSerializer.Deserialize<EmptyString>("\"a\"");

        // Act
        EmptyString result = new(string.Empty);

        // Assert
        result.Value.Should().BeEmpty();
        constructor.Should().ThrowExactly<ArgumentException>();
        conversion.Should().ThrowExactly<ArgumentException>();
        deserialize.Should().ThrowExactly<JsonException>().WithInnerException<ArgumentException>();
    }

    [TestMethod]
    public void Omitted_Limit_Should_Allow_Long_Strings()
    {
        // Arrange
        string value = new('x', 10000);

        // Act
        TestString result = new(value);

        // Assert
        result.Value.Should().Be(value);
    }

    [TestMethod]
    public void Length_Should_Count_Utf16_Code_Units()
    {
        // Arrange
        Action action = () => _ = new SingleCodeUnitString("\U0001F600");

        // Act
        SingleCodeUnitString result = new("a");

        // Assert
        result.Value.Should().Be("a");
        action.Should().ThrowExactly<ArgumentException>();
    }

    [TestMethod]
    public void Custom_Email_Limit_Should_Allow_Boundary_And_Reject_Longer_Address()
    {
        // Arrange
        Action action = () => _ = new ShortEmail("ab@c.de");

        // Act
        ShortEmail result = new("a@b.co");

        // Assert
        result.Value.Should().Be("a@b.co");
        action.Should().ThrowExactly<ArgumentException>();
    }

    [TestMethod]
    public void Default_Email_Limit_Should_Allow_254_And_Reject_255_Code_Units()
    {
        // Arrange
        string domain = new string('b', 63) + "." + new string('c', 63) + "." + new string('d', 61);
        string address = new string('a', 64) + "@" + domain;
        Action action = () => _ = new TestEmail(address + "e");

        // Act
        TestEmail result = new(address);

        // Assert
        result.Value.Length.Should().Be(254);
        action.Should().ThrowExactly<ArgumentException>();
    }

    [TestMethod]
    public void Zero_Email_Limit_Should_Still_Require_Valid_Email()
    {
        // Arrange
        Action empty = () => _ = new ZeroLengthEmail(string.Empty);
        Action nonEmpty = () => _ = new ZeroLengthEmail("a@b.co");

        // Act / Assert
        empty.Should().ThrowExactly<FormatException>();
        nonEmpty.Should().ThrowExactly<ArgumentException>();
    }
}
