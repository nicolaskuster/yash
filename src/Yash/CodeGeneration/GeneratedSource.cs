namespace Yash.CodeGeneration;

using Microsoft.CodeAnalysis.Text;

public record GeneratedSource(string HintName, SourceText SourceText)
{
    public string HintName { get; } = HintName;

    public SourceText SourceText { get; } = SourceText;

    public void Deconstruct(out string hintName, out SourceText sourceText)
    {
        hintName = HintName;
        sourceText = SourceText;
    }
}