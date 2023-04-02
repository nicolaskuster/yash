namespace Yash.UnitTest.SourceGenerators;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

public static class CompilationFactory
{
    public static CSharpCompilation New(string sourceCode)
    {
        return CSharpCompilation.Create(
            "compilation",
            new[] { CSharpSyntaxTree.ParseText(sourceCode) },
            new[] { MetadataReference.CreateFromFile(typeof(object).Assembly.Location) },
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));
    }
}