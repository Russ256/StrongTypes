namespace StrongTypes.Tests;

using FluentAssertions;
using StrongTypes.CompilationTests;

[TestClass]
public class ComparisonTests
{
    [TestMethod]
    public void Value_Wrappers_Should_Implement_Consistent_Typed_Equality_And_Ordering()
    {
        // Arrange / Act / Assert
        Verify(new TestInt(42), new TestInt(42), new TestInt(43));
        Verify(new IntegerClassId(42), new IntegerClassId(42), new IntegerClassId(43));
        Guid next = new("00000000-0000-0000-0000-000000000001");
        Verify(new TestGuidStruct(Guid.Empty), new TestGuidStruct(Guid.Empty), new TestGuidStruct(next));
        Verify(new TestGuidClass(Guid.Empty), new TestGuidClass(Guid.Empty), new TestGuidClass(next));
    }

    [TestMethod]
    public void Class_Typed_Equality_When_Null_Should_Return_False()
    {
        // Arrange
        TestGuidClass guid = new(Guid.Empty);
        IntegerClassId integer = new(42);

        // Act / Assert
        ((IEquatable<TestGuidClass>)guid).Equals(null).Should().BeFalse();
        ((IEquatable<IntegerClassId>)integer).Equals(null).Should().BeFalse();
    }

    [TestMethod]
    public void Records_Should_Preserve_Typed_Equality_Without_Implicit_Value_Ordering()
    {
        // Arrange
        IntegerRecord first = new(42) { Label = "first" };
        IntegerRecord second = new(42) { Label = "second" };
        HashSet<IntegerRecord> records = [first, second, new DerivedIntegerRecord(42)];

        // Act / Assert
        ((IEquatable<IntegerRecord>)first).Equals(first with { }).Should().BeTrue();
        ((IEquatable<IntegerRecord>)first).Equals(second).Should().BeFalse();
        records.Should().HaveCount(3);
        typeof(IComparable<IntegerRecord>).IsAssignableFrom(typeof(IntegerRecord)).Should().BeFalse();
        typeof(IComparable<SealedGuidRecord>).IsAssignableFrom(typeof(SealedGuidRecord)).Should().BeFalse();
        typeof(IComparable<TestGuidRecord>).IsAssignableFrom(typeof(TestGuidRecord)).Should().BeFalse();
        typeof(IEquatable<TestGuidRecord>).IsAssignableFrom(typeof(TestGuidRecord)).Should().BeTrue();
    }

    private static void Verify<T>(T value, T equal, T greater)
        where T : IEquatable<T>, IComparable<T>
    {
        value.Equals(equal).Should().BeTrue();
        value.Equals(greater).Should().BeFalse();
        value.Equals((object)equal).Should().BeTrue();
        value.Equals((object)greater).Should().BeFalse();
        value.Equals(new object()).Should().BeFalse();
        value.GetHashCode().Should().Be(equal.GetHashCode());
        value.CompareTo(equal).Should().Be(0);
        value.CompareTo(greater).Should().BeNegative();
        greater.CompareTo(value).Should().BePositive();
        new HashSet<T> { value, equal, greater }.Should().HaveCount(2);
        new SortedSet<T> { value, equal, greater }.Should().HaveCount(2);
    }
}
