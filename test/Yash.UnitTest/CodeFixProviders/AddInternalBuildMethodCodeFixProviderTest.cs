#pragma warning disable S2699 // Add at least one assertion to this test case.

namespace Yash.UnitTest.CodeFixProviders;

using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Testing;
using NUnit.Framework;
using Yash.Analyzers;
using Yash.CodeFixProviders;

public class AddInternalBuildMethodCodeFixProviderTest
{
    [Test]
    public async Task FixAsync_AddInternalBuildMethod_WhenClassHasNoInternalBuildMethod()
    {
        // lang=c#
        var sourceCode = @"
            namespace MyNamespace;

            using global::Yash.Generated.Attributes;

            [Yash<string>]
            public partial class MyBuilder
            {

            }
        ";

        // lang=c#
        var expectedCode = @"
            namespace MyNamespace;

            using global::Yash.Generated.Attributes;

            [Yash<string>]
            public partial class MyBuilder
            {
private static string BuildInternal() => throw new System.NotImplementedException();

            }
        ";

        var expectedDiagnostic = new DiagnosticResult(
                Constants.YashClassWithMissingInternalBuildMethodDiagnosticId,
                DiagnosticSeverity.Error)
            .WithLocation(path: "/0/Test0.cs", line: 7, column: 34);

        var test = new CSharpCodeFixTest<YashAnalyzer, AddInternalBuildMethodCodeFixProvider>
        {
            TestCode = sourceCode,
            FixedCode = expectedCode,
            ExpectedDiagnostics = { expectedDiagnostic },
        };

        await test.RunAsync();
    }
}