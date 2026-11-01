namespace StrongTypes.Tests;

using FluentAssertions;

[TestClass]
public class NullabilityTests
{
    [TestMethod]
    [DataRow(false, false, true)]
    [DataRow(false, true, false)]
    [DataRow(true, false, false)]
    [DataRow(true, true, true)]
    public void Guid_Class_Equality_Should_Accept_Nullable_Operands(bool hasLeft, bool hasRight, bool expected)
    {
        // Arrange
        TestGuidClass? left = hasLeft ? new(Guid.Empty) : null;
        TestGuidClass? right = hasRight ? new(Guid.Empty) : null;

        // Assert
        (left == right).Should().Be(expected);
        (left != right).Should().Be(!expected);
        (left == null).Should().Be(!hasLeft);
        (null == left).Should().Be(!hasLeft);
        (left != null).Should().Be(hasLeft);
        (null != left).Should().Be(hasLeft);
    }

    [TestMethod]
    public void Reference_Types_Should_Compare_Greater_Than_Null()
    {
        // Arrange
        TestGuidClass guidClass = new(Guid.Empty);
        IntegerRecord integerRecord = new(0);
        SealedGuidRecord guidRecord = new(Guid.Empty);

        // Assert
        guidClass.CompareTo(null).Should().BePositive();
        integerRecord.CompareTo(null).Should().BePositive();
        guidRecord.CompareTo(null).Should().BePositive();
    }

    [TestMethod]
    public void Guid_Class_TryParse_Should_Return_Null_On_Failure_And_NonNull_On_Success()
    {
        // Act / Assert
        TestGuidClass.TryParse("invalid", out TestGuidClass? invalid).Should().BeFalse();
        invalid.Should().BeNull();
        TestGuidClass.TryParse(null, null, out TestGuidClass? missing).Should().BeFalse();
        missing.Should().BeNull();

        if (TestGuidClass.TryParse(Guid.Empty.ToString(), out TestGuidClass? result))
        {
            result.Value.Should().Be(Guid.Empty);
        }
        else
        {
            Assert.Fail("Expected a valid GUID to parse.");
        }
    }

    [TestMethod]
    public void Integer_Record_TryParse_Should_Return_Null_On_Failure_And_NonNull_On_Success()
    {
        // Act / Assert
        IntegerRecord.TryParse("invalid", out IntegerRecord? invalid).Should().BeFalse();
        invalid.Should().BeNull();
        IntegerRecord.TryParse(null, null, out IntegerRecord? missing).Should().BeFalse();
        missing.Should().BeNull();

        if (IntegerRecord.TryParse("42", null, out IntegerRecord? result))
        {
            result.Value.Should().Be(42);
        }
        else
        {
            Assert.Fail("Expected a valid integer to parse.");
        }
    }
}
