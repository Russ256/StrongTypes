namespace StrongTypes.Tests;

using FluentAssertions;
using System.Text.Json;

[TestClass]
public class JsonValidationTests
{
    [TestMethod]
    [DataRow(typeof(TestEmail), "\"invalid\"", typeof(FormatException))]
    [DataRow(typeof(TestStringLength), "\"abcd\"", typeof(ArgumentException))]
    [DataRow(typeof(TestGuidClass), "\"invalid\"", typeof(ArgumentException))]
    [DataRow(typeof(TestGuidStruct), "\"invalid\"", typeof(ArgumentException))]
    [DataRow(typeof(TestGuidStruct), "null", typeof(ArgumentNullException))]
    [DataRow(typeof(TestUriClass), "\"http://[\"", typeof(UriFormatException))]
    public void Deserialize_Invalid_Value_Should_Throw_JsonException_With_Path_And_Inner_Exception(
        Type strongType, string jsonValue, Type exceptionType)
    {
        // Arrange
        string json = "[" + jsonValue + "]";
        Action action = () => JsonSerializer.Deserialize(json, strongType.MakeArrayType());

        // Act
        JsonException exception = action.Should().ThrowExactly<JsonException>().Which;

        // Assert
        exception.Path.Should().Be("$[0]");
        exception.InnerException.Should().BeOfType(exceptionType);
    }

    [TestMethod]
    [DataRow(typeof(TestString))]
    [DataRow(typeof(TestEmail))]
    [DataRow(typeof(TestGuidClass))]
    [DataRow(typeof(TestUriClass))]
    public void Deserialize_Null_Reference_Type_Should_Return_Null(Type strongType)
    {
        // Arrange
        string json = "null";

        // Act
        object? result = JsonSerializer.Deserialize(json, strongType);

        // Assert
        result.Should().BeNull();
    }
}
