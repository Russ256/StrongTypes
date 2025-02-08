namespace StrongTypes.Tests;

using FluentAssertions;
using System;

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
    public void New_Should_Not_Be_Empty()
    {
        // Act
        TestId id = TestId.New();

        // Assert
        id.Should().NotBeSameAs(TestId.Empty());
    }
}

[Guid]
public partial struct TestId
{
}