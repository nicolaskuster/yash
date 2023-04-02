namespace Yash.CodeGeneration;

using System.Text;
using Microsoft.CodeAnalysis.Text;

public static class YashAttributeGenerator
{
    public static GeneratedSource Generate()
    {
        // language=C#
        var sourceText = SourceText.From(
            $@"
{Constants.GeneratedComment}
{Constants.EnableNullable}

namespace {Constants.AttributeNamespace};

{Constants.GeneratedCodeAttribute}
[global::System.AttributeUsage(global::System.AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
internal sealed class {Constants.AttributeName}<T> : global::System.Attribute
{{
}}
",
            Encoding.UTF8);

        return new GeneratedSource(
            $"{Constants.AttributeName}.{Constants.FileHintSuffix}",
            sourceText);
    }
}