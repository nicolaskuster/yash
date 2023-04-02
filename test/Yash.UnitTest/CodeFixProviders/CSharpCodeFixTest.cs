namespace Yash.UnitTest.CodeFixProviders;

using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp.Testing;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Testing.Verifiers;
using Yash.CodeGeneration;
using Yash.SourceGenerators;

public class CSharpCodeFixTest<TAnalyzer, TCodeFix>
    : CSharpCodeFixTest<TAnalyzer, TCodeFix, NUnitVerifier>
    where TAnalyzer : DiagnosticAnalyzer, new()
    where TCodeFix : CodeFixProvider, new()
{
    public new async Task RunAsync(CancellationToken cancellationToken = default)
    {
        var generatedAttribute = YashAttributeGenerator.Generate();

        TestState.Sources.Add((typeof(YashSourceGenerator), generatedAttribute.HintName,
            generatedAttribute.SourceText));
        FixedState.Sources.Add((typeof(YashSourceGenerator), generatedAttribute.HintName,
            generatedAttribute.SourceText));

        await base.RunAsync(cancellationToken);
    }
}