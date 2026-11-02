namespace StrongTypes.Tests;

using System;
using System.Linq;
using FluentAssertions;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using StrongTypes.EntityFrameworkCore;

[TestClass]
public class EntityFrameworkCoreTests
{
    [TestMethod]
    [DataRow("https://example.com/path?q=1")]
    [DataRow("../relative/path?q=1")]
    public void AddStrongTypes_Should_RoundTrip_All_Base_Types_And_Query_Keys(string uri)
    {
        // Arrange
        using SqliteConnection connection = new("Data Source=:memory:");
        connection.Open();
        using StrongTypesContext context = new(connection);
        context.Database.EnsureCreated();
        TestGuidStruct id = new(Guid.NewGuid());
        StrongTypesEntity entity = new()
        {
            Id = id,
            Number = new TestInt(42),
            Record = new TestGuidRecord(Guid.NewGuid()),
            ClassId = new TestGuidClass(Guid.NewGuid()),
            Name = new TestString("Alice"),
            Email = new TestEmail("alice@example.com"),
            Link = new TestUriClass(uri),
            OptionalNumber = new TestInt(7),
            OptionalName = new TestString("optional"),
            ForeignId = id,
            ExternalId = new StrongTypes.CompilationTests.GuidId(Guid.NewGuid())
        };
        context.Entities.Add(entity);
        context.SaveChanges();
        context.ChangeTracker.Clear();

        // Act
        StrongTypesEntity loaded = context.Entities.Single(value => value.Id == id);

        // Assert
        loaded.Id.Should().Be(id);
        loaded.Number.Value.Should().Be(42);
        loaded.Record.Should().Be(entity.Record);
        loaded.ClassId!.Value.Should().Be(entity.ClassId.Value);
        loaded.Name!.Value.Should().Be("Alice");
        loaded.Email!.Value.Should().Be("alice@example.com");
        loaded.Link!.Value.OriginalString.Should().Be(uri);
        loaded.OptionalNumber.Should().Be(new TestInt(7));
        loaded.OptionalName!.Value.Should().Be("optional");
        loaded.ForeignId.Should().Be(entity.ForeignId);
        loaded.ExternalId.Should().Be(entity.ExternalId);
        context.Entities.Count(value => value.ForeignId == id).Should().Be(1);
        context.Model.FindEntityType(typeof(StrongTypesEntity))!
            .FindProperty(nameof(StrongTypesEntity.Link))!
            .GetTypeMapping().Converter!.ProviderClrType.Should().Be(typeof(string));
    }

    [TestMethod]
    public void AddStrongTypes_Should_Preserve_Nulls_And_Track_Replacement_Values()
    {
        // Arrange
        using SqliteConnection connection = new("Data Source=:memory:");
        connection.Open();
        using StrongTypesContext context = new(connection);
        context.Database.EnsureCreated();
        StrongTypesEntity entity = new() { Id = new TestGuidStruct(Guid.NewGuid()) };
        context.Add(entity);
        context.SaveChanges();
        context.ChangeTracker.Clear();
        StrongTypesEntity loaded = context.Entities.Single();
        loaded.OptionalNumber.Should().BeNull();
        loaded.OptionalName.Should().BeNull();
        loaded.ClassId.Should().BeNull();
        loaded.Link.Should().BeNull();

        // Act
        loaded.OptionalName = new TestString("updated");
        loaded.OptionalNumber = new TestInt(10);
        context.SaveChanges();
        context.ChangeTracker.Clear();
        StrongTypesEntity updated = context.Entities.Single();

        // Assert
        updated.OptionalName!.Value.Should().Be("updated");
        updated.OptionalNumber.Should().Be(new TestInt(10));
    }

    [TestMethod]
    public void Converter_Should_Enforce_Constructor_Validation_On_Read()
    {
        // Arrange
        StrongTypeConverter<TestEmail, string> converter = new();

        // Act
        Action deserialize = () => converter.ConvertFromProvider("invalid email");

        // Assert
        deserialize.Should().Throw<FormatException>();
    }

    private sealed class StrongTypesContext(SqliteConnection connection) : DbContext
    {
        public DbSet<StrongTypesEntity> Entities => Set<StrongTypesEntity>();

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
            => optionsBuilder.UseSqlite(connection);

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<StrongTypesEntity>().HasOne<StrongTypesEntity>()
                .WithMany().HasForeignKey(value => value.ForeignId);
        }

        protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder)
        {
            base.ConfigureConventions(configurationBuilder);
            // Include a second generated assembly and a duplicate to exercise assembly discovery.
            configurationBuilder.AddStrongTypes(typeof(TestGuidStruct).Assembly,
                typeof(StrongTypes.CompilationTests.GuidId).Assembly, typeof(TestGuidStruct).Assembly);
        }
    }

    private sealed class StrongTypesEntity
    {
        public TestGuidStruct Id { get; set; }
        public TestInt Number { get; set; }
        public TestGuidRecord Record { get; set; }
        public TestGuidClass? ClassId { get; set; }
        public TestString? Name { get; set; }
        public TestEmail? Email { get; set; }
        public TestUriClass? Link { get; set; }
        public TestInt? OptionalNumber { get; set; }
        public TestString? OptionalName { get; set; }
        public TestGuidStruct? ForeignId { get; set; }
        public StrongTypes.CompilationTests.GuidId ExternalId { get; set; }
    }
}
