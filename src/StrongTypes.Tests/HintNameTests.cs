namespace StrongTypes.Tests.Sales
{
    [StrongType(BaseType.Integer)]
    public readonly partial struct CustomerId
    {
    }
}

namespace StrongTypes.Tests.Support
{
    [StrongType(BaseType.Guid)]
    public readonly partial struct CustomerId
    {
    }
}

namespace StrongTypes.Tests
{
    using FluentAssertions;

    [TestClass]
    public class HintNameTests
    {
        [TestMethod]
        public void Same_Name_In_Different_Namespaces_Should_Generate_Both_Types()
        {
            // Arrange
            Guid supportId = Guid.NewGuid();

            // Act
            Sales.CustomerId sales = new(42);
            Support.CustomerId support = new(supportId);

            // Assert
            sales.Value.Should().Be(42);
            support.Value.Should().Be(supportId);
        }
    }
}
