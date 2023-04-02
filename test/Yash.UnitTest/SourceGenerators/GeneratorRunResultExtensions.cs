namespace Yash.UnitTest.SourceGenerators;

using System.Linq;
using System.Text;
using FluentAssertions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

public static class GeneratorRunResultExtensions
{
    public static void ShouldHaveGeneratedSource(
        this GeneratorRunResult result,
        string hintName,
        string expectedSourceCode)
    {
        var expectedSyntaxTree = CSharpSyntaxTree.ParseText(expectedSourceCode, encoding: Encoding.UTF8);
        result.GeneratedSources.Should().ContainSingle(gs =>
            gs.HintName == hintName
            && SyntaxFactory.AreEquivalent(
                gs.SyntaxTree.GetRoot(default),
                expectedSyntaxTree.GetRoot(default),
                _ => false));
    }

    public static void ShouldHaveAllTrackedOutputStepsReasonsBeCached(this GeneratorRunResult result)
    {
        var allOutputs = result.TrackedOutputSteps
            .SelectMany(outputStep => outputStep.Value)
            .SelectMany(output => output.Outputs);

        allOutputs.Should().AllSatisfy(o => o.Reason.Should().Be(IncrementalStepRunReason.Cached));
    }

    public static void ShouldNotHaveAException(this GeneratorRunResult result)
    {
        result.Exception.Should().BeNull();
    }

    public static void ShouldNotHaveDiagnostics(this GeneratorRunResult result)
    {
        result.Diagnostics.Should().BeEmpty();
    }
}