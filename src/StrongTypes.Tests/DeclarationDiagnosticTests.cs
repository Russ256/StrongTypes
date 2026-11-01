namespace StrongTypes.Tests;

using FluentAssertions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using System.Linq;

[TestClass]
[DoNotParallelize]
public class DeclarationDiagnosticTests
{
    [TestMethod]
    [DataRow("[StrongType(BaseType.Integer)] public partial struct Id { }", "ST0005")]
    [DataRow("namespace Example { public class Outer { [StrongType(BaseType.Integer)] public partial struct Id { } } }", "ST0006")]
    [DataRow("namespace Example { [StrongType(BaseType.Integer)] public partial struct Id<T> { } }", "ST0007")]
    [DataRow("namespace Example { [StrongType(BaseType.String)] public partial struct Id { } }", "ST0008")]
    [DataRow("namespace Example { [StrongType(BaseType.EmailAddress)] public partial struct Id { } }", "ST0008")]
    [DataRow("namespace Example { [StrongType(BaseType.String)] public partial record Id; }", "ST0008")]
    [DataRow("namespace Example { [StrongType(BaseType.EmailAddress)] public partial record struct Id; }", "ST0008")]
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
}
