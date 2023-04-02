#pragma warning disable S2699 // Add at least one assertion to this test case.
namespace Yash.UnitTest.CodeFixProviders;

using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Testing;
using NUnit.Framework;
using Yash.Analyzers;
using Yash.CodeFixProviders;

public class MakeClassNonStaticCodeFixProviderTest
{
    [Test]
    public async Task FixAsync_MakesClassPartial_WhenClassIsNotPartial()
    {
        // lang=c#
        var sourceCode = @"
            namespace MyNamespace;

            using global::Yash.Generated.Attributes;

            [Yash<string>]
            public static partial class MyBuilder
            {
                private static string BuildInternal() => ""JustAStaticString"";
            }
        ";

        // lang=c#
        var expectedCode = @"
            namespace MyNamespace;

            using global::Yash.Generated.Attributes;

            [Yash<string>]
            public partial class MyBuilder
            {
                private static string BuildInternal() => ""JustAStaticString"";
            }
        ";

        var expectedDiagnostic = new DiagnosticResult(
                Constants.YashClassCantBeStaticDiagnosticId,
                DiagnosticSeverity.Warning)
            .WithLocation(path: "/0/Test0.cs", line: 7, column: 20);

        var test = new CSharpCodeFixTest<YashAnalyzer, MakeClassNonStaticCodeFixProvider>
        {
            TestCode = sourceCode,
            FixedCode = expectedCode,
            ExpectedDiagnostics = { expectedDiagnostic },
        };

        await test.RunAsync();
    }
}