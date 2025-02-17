namespace StrongTypes.Tests;

using FluentAssertions;
using System;
using System.Text.Json;

[StrongType(typeof(Guid))]
public readonly partial struct TestGuidStruct
{
}

[StrongType(typeof(Guid))]
public readonly partial record struct TestGuidRecord
{
}

[TestClass]
public class GuidTests
{
    [TestMethod]
    public void CompareTo_When_LessThan_Returns_Negative()
    {
        // Arrange
        TestGuidStruct sut1 = new("3289446b-e0d8-485e-a5a6-0e2a2b986dee");
        TestGuidStruct sut2 = new("3289446b-e0d8-485e-a5a6-0e2a2b986def");

        // Act
        int result = sut1.CompareTo(sut2);

        // Assert
        result.Should().BeNegative();
    }

    [TestMethod]
    public void CompareTo_When_MoreThan_Returns_Positive()
    {
        // Arrange
        TestGuidStruct sut1 = new("3289446b-e0d8-485e-a5a6-0e2a2b986dee");
        TestGuidStruct sut2 = new("3289446b-e0d8-485e-a5a6-0e2a2b986dea");

        // Act
        int result = sut1.CompareTo(sut2);

        // Assert
        result.Should().BePositive();
    }

    [TestMethod]
    public void CompareTo_When_Same_Guid_Returns_0()
    {
        // Arrange
        Guid guid = Guid.NewGuid();
        TestGuidStruct sut1 = new(guid);
        TestGuidStruct sut2 = new(guid);

        // Act
        int result = sut1.CompareTo(sut2);

        // Assert
        result.Should().Be(0);
    }

    [TestMethod]
    public void Default_Constructor_Should_Not_Be_Empty()
    {
        // Act
        TestGuidStruct sut = new();

        // Assert
        sut.Should().NotBeSameAs(TestGuidStruct.Empty);
        sut.ToString().Should().Be("00000000-0000-0000-0000-000000000000");
    }

    [TestMethod]
    public void Default_Should_Be_Empty()
    {
        // Act
        TestGuidStruct sut = default;

        // Assert
        sut.Should().Be(TestGuidStruct.Empty);
    }

    [TestMethod]
    public void Equals_Operator_When_Not_Same_Returns_False()
    {
        // Arrange
        TestGuidStruct sut1 = TestGuidStruct.New();
        TestGuidStruct sut2 = TestGuidStruct.New();

        // Act
        bool result = sut1 == sut2;

        // Assert
        result.Should().BeFalse();
    }

    [TestMethod]
    public void Record_Equals_Operator_When_Not_Same_Returns_False()
    {
        // Arrange
        TestGuidRecord sut1 = TestGuidRecord.New();
        TestGuidRecord sut2 = TestGuidRecord.New();

        // Act
        bool result = sut1 == sut2;

        // Assert
        result.Should().BeFalse();
    }

    [TestMethod]
    public void Equals_Operator_When_Same_Returns_true()
    {
        // Arrange
        Guid guid = Guid.NewGuid();
        TestGuidStruct sut1 = new(guid);
        TestGuidStruct sut2 = new(guid);

        // Act
        bool result = sut1 == sut2;

        // Assert
        result.Should().BeTrue();
    }

    [TestMethod]
    public void Recrod_Equals_Operator_When_Same_Returns_true()
    {
        // Arrange
        Guid guid = Guid.NewGuid();
        TestGuidRecord sut1 = new(guid);
        TestGuidRecord sut2 = new(guid);

        // Act
        bool result = sut1 == sut2;

        // Assert
        result.Should().BeTrue();
    }

    [TestMethod]
    public void Guid_Constructor_Should_Match_Guid()
    {
        // Arrange
        Guid guid = Guid.NewGuid();

        // Act
        TestGuidStruct sut = new(guid);

        // Assert
        sut.ToString().Should().Be(guid.ToString());
    }

    [TestMethod]
    public void Json_Deserialize_Should_Succeed()
    {
        // Arrange
        Guid guid = Guid.NewGuid();
        string json = $"\"{guid}\"";

        // Act
        TestGuidStruct result = JsonSerializer.Deserialize<TestGuidStruct>(json);

        // Assert
        result.ToString().Should().Be(guid.ToString());
    }

    [TestMethod]
    public void Json_Serialize_Should_Include_Quotes()
    {
        // Arrange
        TestGuidStruct sut = TestGuidStruct.New();

        // Act
        string result = JsonSerializer.Serialize(sut);

        // Assert
        result.Should().Be($"\"{sut}\"");
    }

    [TestMethod]
    public void New_Should_Not_Be_Empty()
    {
        // Act
        TestGuidStruct sut = TestGuidStruct.New();

        // Assert
        sut.Should().NotBeSameAs(TestGuidStruct.Empty);
    }

    [TestMethod]
    public void NotEquals_Operator_When_Not_Same_Returns_True()
    {
        // Arrange
        TestGuidStruct sut1 = TestGuidStruct.New();
        TestGuidStruct sut2 = TestGuidStruct.New();

        // Act
        bool result = sut1 != sut2;

        // Assert
        result.Should().BeTrue();
    }

    [TestMethod]
    public void NotEquals_Operator_When_Same_Returns_False()
    {
        // Arrange
        Guid guid = Guid.NewGuid();
        TestGuidStruct sut1 = new(guid);
        TestGuidStruct sut2 = new(guid);

        // Act
        bool result = sut1 != sut2;

        // Assert
        result.Should().BeFalse();
    }

    [TestMethod]
    public void NullGuid_Constructor_Should_Match_Guid()
    {
        // Act
        Action action = () => _ = new TestGuidStruct(null!);

        // Assert
        action.Should().Throw<ArgumentNullException>().WithMessage("Value cannot be null. (Parameter 'value')");
    }

    [TestMethod]
    public void Parse_When_Invalid_Guid_Throws_Exception()
    {
        // Arrange
        string value = "test";

        // Act
        Action action = () => TestGuidStruct.Parse(value);

        // Assert
        action.Should().Throw<FormatException>().WithMessage($"'{value}' is not a valid TestGuidStruct.");
    }

    [TestMethod]
    public void Parse_When_Valid_Guid_Returns_True_And_New_Type()
    {
        // Arrange
        string value = Guid.NewGuid().ToString();

        // Act
        TestGuidStruct result = TestGuidStruct.Parse(value);

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
        TestGuidStruct sut = new(guid.ToString(format));

        // Assert
        sut.ToString().Should().Be(guid.ToString());
    }

    [TestMethod]
    public void TryParse_When_Invalid_Guid_Returns_False_And_Default_Object()
    {
        // Arrange
        string value = "test";

        // Act
        bool result = TestGuidStruct.TryParse(value, out TestGuidStruct sut);

        // Assert
        result.Should().BeFalse();
        sut.Should().Be(TestGuidStruct.Empty);
    }

    [TestMethod]
    public void TryParse_When_Valid_Guid_Returns_True_And_New_Type()
    {
        // Arrange
        string value = Guid.NewGuid().ToString();

        // Act
        bool result = TestGuidStruct.TryParse(value, out TestGuidStruct id);

        // Assert
        result.Should().BeTrue();
        id.ToString().Should().Be(value);
    }
}