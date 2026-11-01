namespace StrongTypes.Tests;

using FluentAssertions;

[StrongType(BaseType.Integer)]
public partial record IntegerRecord
{
    public string? Label { get; init; }
}

[StrongType(BaseType.Guid)]
public sealed partial record class SealedGuidRecord
{
}

public record DerivedIntegerRecord : IntegerRecord
{
    public DerivedIntegerRecord(int value) : base(value) { }
}

[TestClass]
public class RecordEqualityTests
{
    [TestMethod]
    public void Unsealed_Record_Equality_Should_Compare_Values_And_Handle_Null()
    {
        // Arrange
        IntegerRecord left = new(42);
        IntegerRecord equal = new(42);
        IntegerRecord different = new(43);

        // Assert
        left.Equals(equal).Should().BeTrue();
        (left == equal).Should().BeTrue();
        left.GetHashCode().Should().Be(equal.GetHashCode());
        left.Equals(different).Should().BeFalse();
        left.Equals((IntegerRecord?)null).Should().BeFalse();
        (left == null).Should().BeFalse();
        (left != null).Should().BeTrue();
    }

    [TestMethod]
    public void Sealed_Record_Equality_Should_Compare_Values_And_Handle_Null()
    {
        // Arrange
        Guid value = Guid.NewGuid();
        SealedGuidRecord left = new(value);
        SealedGuidRecord equal = new(value);

        // Assert
        left.Equals(equal).Should().BeTrue();
        (left == equal).Should().BeTrue();
        left.GetHashCode().Should().Be(equal.GetHashCode());
        left.Equals(new SealedGuidRecord(Guid.Empty)).Should().BeFalse();
        left.Equals((SealedGuidRecord?)null).Should().BeFalse();
        (left == null).Should().BeFalse();
    }

    [TestMethod]
    public void Record_Equality_Should_Include_Additional_State_And_Preserve_With_Copies()
    {
        // Arrange
        IntegerRecord original = new(42) { Label = "first" };
        IntegerRecord copy = original with { };
        IntegerRecord changed = original with { Label = "second" };

        // Assert
        original.Equals(copy).Should().BeTrue();
        original.GetHashCode().Should().Be(copy.GetHashCode());
        original.Equals(changed).Should().BeFalse();
        changed.Value.Should().Be(42);
    }

    [TestMethod]
    public void Record_Equality_Should_Distinguish_Base_And_Derived_Records()
    {
        // Arrange
        IntegerRecord parent = new(42);
        IntegerRecord child = new DerivedIntegerRecord(42);

        // Assert
        parent.Equals(child).Should().BeFalse();
        child.Equals(parent).Should().BeFalse();
        (parent == child).Should().BeFalse();
    }
}
