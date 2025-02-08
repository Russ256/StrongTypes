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

    [TestMethod]
    public void CompareTo_When_Same_Guid_Returns_0()
    {
        // Arrange
        Guid guid = Guid.NewGuid();
        TestId id1 = new(guid);
        TestId id2 = new(guid);

        // Act
        int result = id1.CompareTo(id2);

        // Assert
        result.Should().Be(0);
    }

    [TestMethod]
    public void CompareTo_When_LessThan_Returns_Negative()
    {
        // Arrange
        TestId id1 = new("3289446b-e0d8-485e-a5a6-0e2a2b986dee");
        TestId id2 = new("3289446b-e0d8-485e-a5a6-0e2a2b986def");

        // Act
        int result = id1.CompareTo(id2);

        // Assert
        result.Should().BeNegative();
    }

    [TestMethod]
    public void CompareTo_When_MoreThan_Returns_Positive()
    {
        // Arrange
        TestId id1 = new("3289446b-e0d8-485e-a5a6-0e2a2b986dee");
        TestId id2 = new("3289446b-e0d8-485e-a5a6-0e2a2b986dea");

        // Act
        int result = id1.CompareTo(id2);

        // Assert
        result.Should().BePositive();
    }

    [TestMethod]
    public void Equals_When_Same_Returns_true()
    {
        // Arrange
        Guid guid = Guid.NewGuid();
        TestId id1 = new(guid);
        TestId id2 = new(guid);

        // Act
        bool result = id1 == id2;

        // Assert
        result.Should().BeTrue();
    }

    [TestMethod]
    public void Equals_When_Not_Same_Returns_False()
    {
        // Arrange
        TestId id1 = TestId.New();
        TestId id2 = TestId.New();

        // Act
        bool result = id1 == id2;

        // Assert
        result.Should().BeFalse();
    }
}

[Guid]
public partial struct TestId 
{
  
}