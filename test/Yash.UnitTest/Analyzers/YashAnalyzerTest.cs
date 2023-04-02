#pragma warning disable S2699 // Add at least one assertion to this test case.

namespace Yash.UnitTest.Analyzers;

using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Testing;
using NUnit.Framework;
using Yash.Analyzers;

public class YashAnalyzerTest
{
    [Test]
    public async Task YashAnalyzer_ProducesDiagnostic_WhenUsingYashAttributeOnNonPartialClass()
    {
        // lang=c#
        var sourceCode = @"
            namespace MyNamespace;

            using Yash.Generated.Attributes;

            [Yash<string>]
            public class MyBuilder
            {
                 private static string BuildInternal() => ""JustAStaticString"";
            }
        ";

        var expectedDiagnostic = new DiagnosticResult(
                Constants.YashClassNeedsToBePartialDiagnosticId,
                DiagnosticSeverity.Warning)
            .WithLocation(path: "/0/Test0.cs", line: 7, column: 20);

        var test = new CSharpAnalyzerTest<YashAnalyzer>
        {
            TestCode = sourceCode,
            ExpectedDiagnostics = { expectedDiagnostic },
        };

        await test.RunAsync();
    }

    [Test]
    public async Task YashAnalyzer_ProducesDiagnostic_WhenUsingYashAttributeOnStaticClass()
    {
        // lang=c#
        var sourceCode = @"
            namespace MyNamespace;

            using Yash.Generated.Attributes;

            [Yash<string>]
            public static partial class MyBuilder
            {
                private static string BuildInternal() => ""JustAStaticString"";
            }
        ";

        var expectedDiagnostic = new DiagnosticResult(
                Constants.YashClassCantBeStaticDiagnosticId,
                DiagnosticSeverity.Warning)
            .WithLocation(path: "/0/Test0.cs", line: 7, column: 20);

        var test = new CSharpAnalyzerTest<YashAnalyzer>
        {
            TestCode = sourceCode,
            ExpectedDiagnostics = { expectedDiagnostic },
        };

        await test.RunAsync();
    }

    [Test]
    public async Task
        YashAnalyzer_ProducesDiagnostic_WhenUsingYashAttributeOnClassWithMissingInternalBuildMethodMethod()
    {
        // lang=c#
        var sourceCode = @"
            namespace MyNamespace;

            using Yash.Generated.Attributes;

            [Yash<string>]
            public partial class MyBuilder{}
        ";

        var expectedDiagnostic = new DiagnosticResult(
                Constants.YashClassWithMissingInternalBuildMethodDiagnosticId,
                DiagnosticSeverity.Error)
            .WithLocation(path: "/0/Test0.cs", line: 7, column: 34);

        var test = new CSharpAnalyzerTest<YashAnalyzer>
        {
            TestCode = sourceCode,
            ExpectedDiagnostics = { expectedDiagnostic },
        };

        await test.RunAsync();
    }

    [Test]
    public async Task YashAnalyzer_ProducesDiagnostic_WhenUsingYashAttributeOnClassWithMultipleInternalBuildMethods()
    {
        // lang=c#
        var sourceCode = @"
            namespace MyNamespace;

            using Yash.Generated.Attributes;

            [Yash<string>]
            public partial class MyBuilder{
                private static string BuildInternal() => ""JustAStaticString"";
                private static string BuildInternal(string aString) => aString;
            }
        ";

        var expectedDiagnosticForFirstInternalBuildMethod = new DiagnosticResult(
                Constants.YashClassMultipleInternalBuildMethodsDiagnosticId,
                DiagnosticSeverity.Warning)
            .WithLocation(path: "/0/Test0.cs", line: 8, column: 39);

        var expectedDiagnosticForSecondInternalBuildMethod = new DiagnosticResult(
                Constants.YashClassMultipleInternalBuildMethodsDiagnosticId,
                DiagnosticSeverity.Warning)
            .WithLocation(path: "/0/Test0.cs", line: 9, column: 39);

        var test = new CSharpAnalyzerTest<YashAnalyzer>
        {
            TestCode = sourceCode,
            ExpectedDiagnostics =
            {
                expectedDiagnosticForFirstInternalBuildMethod,
                expectedDiagnosticForSecondInternalBuildMethod,
            },
        };

        await test.RunAsync();
    }
}