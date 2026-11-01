namespace StrongTypes.Tests;

using FluentAssertions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using System.Linq;
using System.Threading.Tasks;

[TestClass]
public class DeclarationDiagnosticTests
{
    [TestMethod]
    public async Task Concurrent_Compilations_Should_Report_Diagnostics_To_Their_Own_Context()
    {
        // Arrange
        Generator generator = new();
        TaskCompletionSource start = new(TaskCreationOptions.RunContinuationsAsynchronously);
        Task[] runs = [.. Enumerable.Range(0, 8).Select(index => Task.Run(async () =>
        {
            string declarations = string.Join(" ", Enumerable.Range(0, 32).Select(typeIndex =>
                index % 2 == 0
                    ? $"[StrongType(BaseType.Integer)] public struct Id{typeIndex} {{ }}"
                    : $"[StrongType(BaseType.Integer)] public partial struct Id{typeIndex}<T> {{ }}"));
            SyntaxTree source = CSharpSyntaxTree.ParseText(
                $"using StrongTypes; namespace Example{index} {{ {declarations} }}");
            CSharpCompilation compilation = CSharpCompilation.Create($"ConcurrentProbe{index}", [source],
                [MetadataReference.CreateFromFile(typeof(object).Assembly.Location)],
                new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));
            GeneratorDriver driver = CSharpGeneratorDriver.Create(generator.AsSourceGenerator());
            await start.Task;

            GeneratorRunResult result = driver.RunGenerators(compilation).GetRunResult().Results.Single();

            // Assert each compilation receives only its own diagnostics.
            result.Exception.Should().BeNull();
            result.Diagnostics.Should().HaveCount(32);
            result.Diagnostics.Should().OnlyContain(diagnostic =>
                diagnostic.Id == (index % 2 == 0 ? "ST0003" : "ST0007")
                && ReferenceEquals(diagnostic.Location.SourceTree, source));
            result.GeneratedSources.Should().HaveCount(2);
        }))];

        // Act
        start.SetResult();
        await Task.WhenAll(runs);
    }

    [TestMethod]
    [DataRow("[StrongType(BaseType.Integer)] public partial struct Id { }", "ST0005")]
    [DataRow("namespace Example { public class Outer { [StrongType(BaseType.Integer)] public partial struct Id { } } }", "ST0006")]
    [DataRow("namespace Example { [StrongType(BaseType.Integer)] public partial struct Id<T> { } }", "ST0007")]
    [DataRow("namespace Example { [StrongType(BaseType.String)] public partial struct Id { } }", "ST0008")]
    [DataRow("namespace Example { [StrongType(BaseType.EmailAddress)] public partial struct Id { } }", "ST0008")]
    [DataRow("namespace Example { [StrongType(BaseType.String)] public partial record Id; }", "ST0008")]
    [DataRow("namespace Example { [StrongType(BaseType.EmailAddress)] public partial record struct Id; }", "ST0008")]
    [DataRow("namespace Example { [StrongType(BaseType.Integer)] file partial struct Id { } }", "ST0009")]
    [DataRow("namespace Example { [StrongType(BaseType.String)] file partial class Id { } }", "ST0009")]
    [DataRow("namespace Example { [StrongType(BaseType.Guid)] file partial record Id; }", "ST0009")]
    [DataRow("namespace Example { [StrongType(BaseType.Integer)] file partial record struct Id; }", "ST0009")]
    public void Unsupported_Declaration_Should_Report_Diagnostic_Without_Emitting_Type(string declaration, string diagnosticId)
    {
        // Arrange
        SyntaxTree source = CSharpSyntaxTree.ParseText("using StrongTypes; " + declaration);
        CSharpCompilation compilation = CSharpCompilation.Create("DeclarationProbe", [source],
            [MetadataReference.CreateFromFile(typeof(object).Assembly.Location)],
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));
        GeneratorDriver driver = CSharpGeneratorDriver.Create(new Generator().AsSourceGenerator());

        // Act
        driver = driver.RunGenerators(compilation);
        GeneratorRunResult result = driver.GetRunResult().Results.Single();

        // Assert
        result.Exception.Should().BeNull();
        Diagnostic diagnostic = result.Diagnostics.Should().ContainSingle().Subject;
        diagnostic.Id.Should().Be(diagnosticId);
        diagnostic.Severity.Should().Be(DiagnosticSeverity.Error);
        diagnostic.Location.SourceTree.Should().BeSameAs(source);
        source.GetText().ToString(diagnostic.Location.SourceSpan).Should().StartWith("StrongType(BaseType.");
        result.GeneratedSources.Select(generated => generated.HintName).Should().BeEquivalentTo(
            "StrongTypes.Attributes.g.cs", "StrongTypes.Exceptions.g.cs");
    }

    [TestMethod]
    public void File_Local_Declaration_Should_Not_Prevent_Generation_For_Valid_Types()
    {
        // Arrange
        SyntaxTree source = CSharpSyntaxTree.ParseText("""
            using StrongTypes;
            namespace Example;
            [StrongType(BaseType.Integer)] file partial struct LocalId { }
            [StrongType(BaseType.Integer)] public partial struct PublicId { }
            """);
        CSharpCompilation compilation = CSharpCompilation.Create("MixedDeclarations", [source],
            [MetadataReference.CreateFromFile(typeof(object).Assembly.Location)],
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));
        GeneratorDriver driver = CSharpGeneratorDriver.Create(new Generator().AsSourceGenerator());

        // Act
        GeneratorRunResult result = driver.RunGenerators(compilation).GetRunResult().Results.Single();

        // Assert
        result.Exception.Should().BeNull();
        result.Diagnostics.Should().ContainSingle().Which.Id.Should().Be("ST0009");
        result.GeneratedSources.Select(generated => generated.HintName).Should().BeEquivalentTo(
            "StrongTypes.Attributes.g.cs", "StrongTypes.Exceptions.g.cs", "Example.PublicId.g.cs");
    }
}
