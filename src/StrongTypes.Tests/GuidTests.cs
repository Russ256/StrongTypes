namespace StrongTypes.Tests;

using FluentAssertions;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json;

[TestClass]
public class GuidTests
{
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
        id.Should().Be(TestId.Empty());
    }

    [TestMethod]
    public void Equals_Operator_When_Not_Same_Returns_False()
    {
        // Arrange
        TestId id1 = TestId.New();
        TestId id2 = TestId.New();

        // Act
        bool result = id1 == id2;

        // Assert
        result.Should().BeFalse();
    }

    [TestMethod]
    public void Equals_Operator_When_Same_Returns_true()
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
    public void New_Should_Not_Be_Empty()
    {
        // Act
        TestId id = TestId.New();

        // Assert
        id.Should().NotBeSameAs(TestId.Empty());
    }

    [TestMethod]
    public void NotEquals_Operator_When_Not_Same_Returns_True()
    {
        // Arrange
        TestId id1 = TestId.New();
        TestId id2 = TestId.New();

        // Act
        bool result = id1 != id2;

        // Assert
        result.Should().BeTrue();
    }

    [TestMethod]
    public void NotEquals_Operator_When_Same_Returns_False()
    {
        // Arrange
        Guid guid = Guid.NewGuid();
        TestId id1 = new(guid);
        TestId id2 = new(guid);

        // Act
        bool result = id1 != id2;

        // Assert
        result.Should().BeFalse();
    }

    [TestMethod]
    public void Parse_When_Invalid_Guid_Throws_Exception()
    {
        // Arrange
        string value = "test";

        // Act
        Action action = () => TestId.Parse(value);

        // Assert
        action.Should().Throw<FormatException>().WithMessage($"'{value}' is not a valid TestId.");
    }

    [TestMethod]
    public void Parse_When_Valid_Guid_Returns_True_And_New_Type()
    {
        // Arrange
        string value = Guid.NewGuid().ToString();

        // Act
        TestId result = TestId.Parse(value);

        // Assert
        result.ToString().Should().Be(value);
    }

    [TestMethod]
    [DataRow("", DisplayName = "Empty")]
    [DataRow("N", DisplayName = "N")]
    [DataRow("D", DisplayName = "D")]
    [DataRow("B", DisplayName = "B")]
    [DataRow("P", DisplayName = "P")]
    [DataRow("X", DisplayName = "X")]
    [DataRow("n", DisplayName = "n")]
    [DataRow("d", DisplayName = "d")]
    [DataRow("b", DisplayName = "b")]
    [DataRow("p", DisplayName = "p")]
    [DataRow("x", DisplayName = "x")]
    public void String_Constructor_Should_Match_Guid(string format)
    {
        // Arrange
        Guid guid = Guid.NewGuid();

        // Act
        TestId id = new(guid.ToString(format));

        // Assert
        id.ToString().Should().Be(guid.ToString());
    }

    [TestMethod]
    public void TryParse_When_Invalid_Guid_Returns_False_And_Default_Object()
    {
        // Arrange
        string value = "test";

        // Act
        bool result = TestId.TryParse(value, out TestId id);

        // Assert
        result.Should().BeFalse();
        id.Should().Be(TestId.Empty());
    }

    [TestMethod]
    public void TryParse_When_Valid_Guid_Returns_True_And_New_Type()
    {
        // Arrange
        string value = Guid.NewGuid().ToString();

        // Act
        bool result = TestId.TryParse(value, out TestId id);

        // Assert
        result.Should().BeTrue();
        id.ToString().Should().Be(value);
    }
}

[Guid]
public partial struct TestId
{
}