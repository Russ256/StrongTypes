namespace StrongTypes.Tests;

using FluentAssertions;
using System.Text.Json;

[TestClass]
public class MemberNameTests
{
    [TestMethod]
    public void Email_Validation_Should_Not_Assign_To_User_Underscore_Member()
    {
        // Arrange
        string address = "user@example.com";
        Action invalid = () => _ = new global::StrongTypes.CompilationTests.EmailWithUnderscoreMember("invalid");

        // Act
        global::StrongTypes.CompilationTests.EmailWithUnderscoreMember sut = new(address);

        // Assert
        sut.UserValue.Should().Be(7);
        sut.Value.Should().Be(address);
        VerifyRoundTrip(sut, "\"user@example.com\"");
        invalid.Should().ThrowExactly<FormatException>();
    }

    [TestMethod]
    public void JsonConverter_Type_Name_Should_Support_All_Base_Types_And_Json_RoundTrips()
    {
        // Arrange / Act / Assert
        VerifyRoundTrip(new MemberNames.Integer.JsonConverter(42), "42");
        VerifyRoundTrip(new MemberNames.Guid.JsonConverter(Guid.Empty), $"\"{Guid.Empty}\"");
        VerifyRoundTrip(new MemberNames.String.JsonConverter("text"), "\"text\"");
        VerifyRoundTrip(new MemberNames.Email.JsonConverter("user@example.com"), "\"user@example.com\"");
        VerifyRoundTrip(new MemberNames.Uri.JsonConverter("https://example.com/"), "\"https://example.com/\"");
    }

    [TestMethod]
    public void Private_Field_And_Converter_Names_Should_Avoid_User_Members()
    {
        // Arrange
        MemberNames.ExistingHelpers sut = new(42);

        // Assert
        sut.UserValue.Should().Be(7);
        sut.Value.Should().Be(42);
        VerifyRoundTrip(sut, "42");
    }

    [TestMethod]
    public void Type_Name_Matching_Private_Field_Should_Support_Generated_Behavior()
    {
        // Arrange
        MemberNames._value sut = new(42);

        // Assert
        sut.Value.Should().Be(42);
        sut.CompareTo(new MemberNames._value(43)).Should().BeNegative();
        VerifyRoundTrip(sut, "42");
    }

    [TestMethod]
    public void Public_Member_Names_Should_Only_Be_Reserved_When_Generated()
    {
        // Act
        MemberNames.String.Parse sut = new("text");

        // Assert
        sut.Value.Should().Be("text");
    }

    [TestMethod]
    public void Records_Named_CompareTo_Should_Support_Generated_Behavior()
    {
        // Arrange
        MemberNames.Integer.CompareTo integer = new(42);
        MemberNames.Guid.CompareTo guid = new(Guid.Empty);

        // Act / Assert
        MemberNames.Integer.CompareTo.Parse("42").Should().Be(integer);
        MemberNames.Guid.CompareTo.Parse(Guid.Empty.ToString()).Should().Be(guid);
        VerifyRoundTrip(integer, "42");
        VerifyRoundTrip(guid, $"\"{Guid.Empty}\"");
    }

    private static void VerifyRoundTrip<T>(T value, string json)
    {
        JsonSerializer.Serialize(value).Should().Be(json);
        JsonSerializer.Deserialize<T>(json).Should().Be(value);
    }
}
