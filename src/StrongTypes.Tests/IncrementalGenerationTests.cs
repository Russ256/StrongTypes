namespace StrongTypes.Tests;

using FluentAssertions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using System.Linq;

[TestClass]
public class IncrementalGenerationTests
{
    [TestMethod]
    [DataRow("unrelated")]
    [DataRow("body")]
    [DataRow("limit")]
    [DataRow("partial")]
    [DataRow("diagnostic")]
    public void Edits_Should_Reuse_Unchanged_Sources_And_Update_Affected_Type(string edit)
    {
        // Arrange
        string declarations = """
            using StrongTypes;
            namespace Example;
            [StrongType(BaseType.String, 3)] public partial class Name { }
            [StrongType(BaseType.Integer)] public partial struct Id { }
            """;
        SyntaxTree annotated = CSharpSyntaxTree.ParseText(declarations, path: "Types.cs");
        SyntaxTree other = CSharpSyntaxTree.ParseText(
            "namespace Example; public partial class Name { } public class Other { }", path: "Other.cs");
        CSharpCompilation compilation = CSharpCompilation.Create("IncrementalProbe", [annotated, other],
            [MetadataReference.CreateFromFile(typeof(object).Assembly.Location)],
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));
        GeneratorDriver driver = CSharpGeneratorDriver.Create(
            generators: [new Generator().AsSourceGenerator()],
            driverOptions: new GeneratorDriverOptions(IncrementalGeneratorOutputKind.None, trackIncrementalGeneratorSteps: true));
        driver = driver.RunGenerators(compilation);
        string originalId = driver.GetRunResult().Results.Single().GeneratedSources
            .Single(source => source.HintName == "Example.Id.g.cs").SourceText.ToString();

        // Act
        compilation = edit switch
        {
            "body" => compilation.ReplaceSyntaxTree(annotated, CSharpSyntaxTree.ParseText(
                declarations.Replace("class Name { }", "class Name { public int Extra => 1; }"), path: "Types.cs")),
            "limit" => compilation.ReplaceSyntaxTree(annotated, CSharpSyntaxTree.ParseText(
                declarations.Replace("String, 3", "String, 4"), path: "Types.cs")),
            "partial" => compilation.ReplaceSyntaxTree(other, CSharpSyntaxTree.ParseText(
                "namespace Example; public partial class Name { private int _value; }", path: "Other.cs")),
            "diagnostic" => compilation.ReplaceSyntaxTree(other, CSharpSyntaxTree.ParseText(
                "namespace Example; public abstract partial class Name { }", path: "Other.cs")),
            _ => compilation.ReplaceSyntaxTree(other, CSharpSyntaxTree.ParseText(
                "namespace Example; public partial class Name { } public class Other { public int X => 1; }", path: "Other.cs")),
        };
        driver = driver.RunGenerators(compilation);
        GeneratorRunResult result = driver.GetRunResult().Results.Single();
        (string HintName, string Source, IncrementalStepRunReason Reason)[] outputs = [.. result.TrackedSteps["StrongTypeSources"]
            .SelectMany(step => step.Outputs)
            .Select(output =>
            {
                (string HintName, string Source) = ((string, string))output.Value;
                return (HintName, Source, output.Reason);
            })];

        // Assert
        result.Exception.Should().BeNull();
        outputs.Single(output => output.HintName == "Example.Id.g.cs").Reason.Should().Be(IncrementalStepRunReason.Cached);
        result.GeneratedSources.Single(source => source.HintName == "Example.Id.g.cs")
            .SourceText.ToString().Should().Be(originalId);
        if (edit == "diagnostic")
        {
            result.Diagnostics.Should().ContainSingle().Which.Id.Should().Be("ST0012");
            result.GeneratedSources.Should().NotContain(source => source.HintName == "Example.Name.g.cs");
        }
        else
        {
            result.Diagnostics.Should().BeEmpty();
            (string HintName, string Source, IncrementalStepRunReason Reason) = outputs.Single(output => output.HintName == "Example.Name.g.cs");
            Reason.Should().Be(edit is "limit" or "partial"
                ? IncrementalStepRunReason.Modified : IncrementalStepRunReason.Cached);
            if (edit == "limit")
            {
                Source.Should().Contain("value.Length > 4");
            }
            if (edit == "partial")
            {
                Source.Should().Contain("private readonly string _value1;");
            }
        }
    }
}
