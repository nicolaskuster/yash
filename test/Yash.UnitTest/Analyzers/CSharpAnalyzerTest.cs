namespace Yash.UnitTest.Analyzers;

using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis.CSharp.Testing;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Testing.Verifiers;
using Yash.CodeGeneration;
using Yash.SourceGenerators;

public class CSharpAnalyzerTest<TAnalyzer>
    : CSharpAnalyzerTest<TAnalyzer, NUnitVerifier>
    where TAnalyzer : DiagnosticAnalyzer, new()
{
    public new async Task RunAsync(CancellationToken cancellationToken = default)
    {
        var generatedAttribute = YashAttributeGenerator.Generate();

        TestState.Sources.Add((typeof(YashSourceGenerator), generatedAttribute.HintName,
            generatedAttribute.SourceText));

        await base.RunAsync(cancellationToken);
    }
}