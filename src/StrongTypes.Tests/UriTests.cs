namespace StrongTypes.Tests;

using FluentAssertions;
using System.Text.Json;

[StrongType(typeof(Uri))]
public partial class TestUriClass
{
}

//[StrongType(typeof(Uri))] // For checking ompiler error
//public partial struct TestUriStruct
//{
//}

[TestClass]
public class UriTests
{
    [TestMethod]
    public void Constructor_With_Uri_Should_Succeed()
    {
        // Arrange
        string uriString = "https://www.untitest.com";
        Uri uri = new(uriString);

        // Act
        TestUriClass sut = new(uri);

        // Assert
        sut.ToString().Should().Be(uri.ToString());
    }

    [TestMethod]
    public void Constructor_With_Valid_String_Should_Succeed()
    {
        // Arrange
        string uriString = "https://www.untitest.com";

        // Act
        TestUriClass sut = new(uriString);

        // Assert
        sut.ToString().Should().Be(uriString + "/");
    }

    [TestMethod]
    public void Equals_Null_Should_Be_False()
    {
        // Arrange
        Uri uri1 = new("https://www.untitest1.com");

        // Act
        bool result = uri1.Equals(null);

        // Assert
        result.Should().BeFalse();
    }

    [TestMethod]
    public void Equals_Operator_Null_Should_Be_False()
    {
        // Arrange
        Uri uri1 = new("https://www.untitest1.com");

        // Act
        bool result = uri1 == null;

        // Assert
        result.Should().BeFalse();
    }

    [TestMethod]
    public void Equals_Operator_With_Differnet_Uri_Should_Be_False()
    {
        // Arrange
        TestUriClass sut1 = new("https://www.untitest1.com");
        TestUriClass sut2 = new("https://www.untitest2.com");

        // Act
        bool result = sut1 == sut2;

        // Assert
        result.Should().BeFalse();
    }

    [TestMethod]
    public void Equals_Operator_With_Same_Uri_Should_Be_True()
    {
        // Arrange
        TestUriClass sut1 = new("https://www.untitest.com");
        TestUriClass sut2 = new("https://www.untitest.com");

        // Act
        bool result = sut1 == sut2;

        // Assert
        result.Should().BeTrue();
    }
    [TestMethod]
    public void Equals_With_Different_Uri_Should_Be_False()
    {
        // Arrange
        Uri uri1 = new("https://www.untitest1.com");
        Uri uri2 = new("https://www.untitest2.com");

        // Act
        bool result = uri1.Equals(uri2);

        // Assert
        result.Should().BeFalse();
    }

    [TestMethod]
    public void Equals_With_Same_Uri_Should_Be_True()
    {
        // Arrange
        string uri = "https://www.untitest.com";
        Uri uri1 = new(uri);
        Uri uri2 = new(uri);

        // Act
        bool result = uri1.Equals(uri2);

        // Assert
        result.Should().BeTrue();
    }
    [TestMethod]
    public void Json_Deserialize_Should_Succeed()
    {
        // Arrange
        Uri uri = new("https://www.unittest.com");
        string json = $"\"{uri}\"";

        // Act
        TestUriClass? result = JsonSerializer.Deserialize<TestUriClass>(json);

        // Assert
        result.Should().NotBeNull();
        result!.ToString().Should().Be(uri.ToString());
    }

    [TestMethod]
    public void Json_Serialize_Should_Include_Quotes()
    {
        // Arrange
        TestUriClass sut = new("https://www.unittest.com/");

        // Act
        string result = JsonSerializer.Serialize(sut);

        // Assert
        result.Should().Be($"\"{sut}\"");
    }
}