namespace Yash.CodeGeneration;

using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis.Text;

public static class YashBuilderGenerator
{
    public static IEnumerable<GeneratedSource> Generate(YashBuilderContext yashBuilderContext)
    {
        yield return GenerateBuilderBaseClass(yashBuilderContext);
        yield return GeneratePartialClass(yashBuilderContext);
    }

    private static GeneratedSource GenerateBuilderBaseClass(YashBuilderContext yashBuilderContext)
    {
        var @namespace = yashBuilderContext.Namespace != null
            ? $"namespace {yashBuilderContext.Namespace};"
            : string.Empty;
        var className = $"{yashBuilderContext.BuilderClassName}{Constants.BaseClassNameSuffix}";

        // language=C#
        var sourceText = SourceText.From(
            $@"
{Constants.GeneratedComment}
{Constants.EnableNullable}

{@namespace}

{Constants.GeneratedCodeAttribute}
{yashBuilderContext.BuilderAccessibility} abstract class {className}<T>
    where T : {className}<T>
{{

    protected sealed record class {Constants.DefaultValuesTypeName}({string.Join(", ", yashBuilderContext.Properties.Select(t => $"{t.Type} {t.Name}"))});

    protected abstract {Constants.DefaultValuesTypeName} {Constants.DefaultValuesPropertyName} {{get;}}

    protected abstract T Instance {{get;}}

    {string.Join("\n\t", yashBuilderContext.Properties.Select(p => $"protected {p.Type}? {p.Name}{Constants.InternalPropertySuffix} = null;"))}

    {string.Join("\n\n\t", yashBuilderContext.Properties.Select(p => @$"public T Set{p.Name.CapitalizeFirstLetter()}({p.Type} value){{
        {p.Name}{Constants.InternalPropertySuffix} = value;
        return Instance;
    }}"))}

}}
",
            Encoding.UTF8);

        return new GeneratedSource(
            $"{className}.{Constants.FileHintSuffix}",
            sourceText);
    }

    private static GeneratedSource GeneratePartialClass(YashBuilderContext yashBuilderContext)
    {
        var @namespace = yashBuilderContext.Namespace != null
            ? $"namespace {yashBuilderContext.Namespace};"
            : string.Empty;
        var className = yashBuilderContext.BuilderClassName;
        var baseClassName = $"{yashBuilderContext.BuilderClassName}{Constants.BaseClassNameSuffix}";

        // language=C#
        var sourceText = SourceText.From(
            $@"
{Constants.GeneratedComment}
{Constants.EnableNullable}

{@namespace}

{Constants.GeneratedCodeAttribute}
{yashBuilderContext.BuilderAccessibility} partial class {className} : {baseClassName}<{className}> {{

    protected override {className} Instance => this;

    public {yashBuilderContext.TargetClassName} Build(){{
        return {Constants.InternalBuildMethodName}(
            {string.Join(",\n\t\t\t", yashBuilderContext.Properties.Select(p => $"{p.Name}{Constants.InternalPropertySuffix} ?? {Constants.DefaultValuesPropertyName}.{p.Name}"))});
    }}

}};
",
            Encoding.UTF8);

        return new GeneratedSource(
            $"{className}.{Constants.FileHintSuffix}",
            sourceText);
    }
}