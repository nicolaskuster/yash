namespace Yash.UnitTest.SourceGenerators;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

public static class GeneratorTestHelper
{
    public static GeneratorDriverRunResult RunGenerator<T>(
        string sourceCode,
        bool withTracking = false,
        string? modifiedSourceCodeForSecondRun = null)
        where T : IIncrementalGenerator, new()
    {
        var compilation = CompilationFactory.New(sourceCode);
        var driver = CSharpGeneratorDriver.Create(
            generators: new[] { new T().AsSourceGenerator() },
            driverOptions: new GeneratorDriverOptions(
                disabledOutputs: default,
                trackIncrementalGeneratorSteps: withTracking)) as GeneratorDriver;

        driver = driver.RunGenerators(compilation);

        if (modifiedSourceCodeForSecondRun is not null)
        {
            var modifiesCompilation = CompilationFactory.New(modifiedSourceCodeForSecondRun);
            driver = driver.RunGenerators(modifiesCompilation);
        }

        return driver.GetRunResult();
    }
}