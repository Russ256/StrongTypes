namespace StrongTypes.Tests;

using FluentAssertions;
using System;
using System.Text.Json;

[TestClass]
public class GuidTests
{
    [TestMethod]
    public void Default_Constructor_Should_Not_Be_Empty()
    {
        // Act
        TestId id = new();

        // Assert
        id.Should().NotBeSameAs(TestId.Empty());
        id.ToString().Should().Be("00000000-0000-0000-0000-000000000000");
    }

    [TestMethod]
    public void Default_Should_Be_Empty()
    {
        // Act
        TestId id = default;

        // Assert
        id.Should().BeSameAs(TestId.Empty());
    }

    [TestMethod]
    public void Guid_Constructor_Should_Match_Guid()
    {
        // Arrange
        Guid guid = Guid.NewGuid();

        // Act
        TestId id = new(guid);

        // Assert
        id.ToString().Should().Be(guid.ToString());
    }

    [TestMethod]
    public void String_Constructor_Should_Match_Guid()
    {
        // Arrange
        Guid guid = Guid.NewGuid();

        // Act 
        TestId id = new(guid.ToString());

        // Assert
        id.ToString().Should().Be(guid.ToString());
    }

    [TestMethod]
    public void New_Should_Not_Be_Empty()
    {
        // Act
        TestId id = TestId.New();

        // Assert
        id.Should().NotBeSameAs(TestId.Empty());
    }

    [TestMethod]
    public void Json_Serialize_Should_Be_Guid()
    {
        // Arrange
        TestId id = TestId.New();

        // Act
        string result = JsonSerializer.Serialize(id);

        // Assert
        result.Should().Be($"\"{id}\"");
    }

    [TestMethod]
    public void Json_Deserialize_Guid_Should_Succeed()
    {
        // Arrange
        Guid guid = Guid.NewGuid();
        string json = $"\"{guid}\"";

        // Act
        TestId result = JsonSerializer.Deserialize<TestId>(json);

        // Assert
        result.ToString().Should().Be(guid.ToString());
    }
}

[Guid]
public partial struct TestId
{
}