namespace Yash.UnitTest.SourceGenerators;

using System.Linq;
using NUnit.Framework;
using Yash.SourceGenerators;

public class YashSourceGeneratorTest
{
    // lang=c#
    private const string SourceCode = @"
            namespace MyNamespace;

            using Yash.Generated.Attributes;

            [Yash<MyClass>]
            public partial class MyBuilder
            {
                private static MyClass BuildInternal(string justAString) => new(justAString);
            }

            public class MyClass
            {
                public MyClass(string justAString){}
            }
        ";

    [Test]
    public void YashSourceGenerator_GeneratesYashAttribute_WhenGeneratorRuns()
    {
        // lang=c#
        var expectedSourceCode = $@"
            // <auto-generated />
            #nullable enable

            namespace Yash.Generated.Attributes;

            {Constants.GeneratedCodeAttribute}
            [global::System.AttributeUsage(global::System.AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
            internal sealed class YashAttribute<T> : global::System.Attribute
            {{
            }}
        ";

        var result = GeneratorTestHelper.RunGenerator<YashSourceGenerator>(string.Empty).Results.Single();

        result.ShouldNotHaveAException();
        result.ShouldNotHaveDiagnostics();
        result.ShouldHaveGeneratedSource("YashAttribute.g.cs", expectedSourceCode);
    }

    [Test]
    public void YashSourceGenerator_GeneratesBaseClass_WhenBuilderClassHasYashAttribute()
    {
        // lang=c#
        var expectedSourceCode = $@"
            // <auto-generated />
            #nullable enable

            namespace MyNamespace;

            {Constants.GeneratedCodeAttribute}
            public abstract class MyBuilderBase<T>
                where T : MyBuilderBase<T>
            {{

                protected sealed record class DefaultValuesRecord(string justAString);

                protected abstract DefaultValuesRecord DefaultValues {{get;}}

                protected abstract T Instance {{get;}}

                protected string? justAStringInternal = null;

                public T SetJustAString(string value){{
                    justAStringInternal = value;
                    return Instance;
                }}

            }}
        ";

        var result = GeneratorTestHelper.RunGenerator<YashSourceGenerator>(SourceCode).Results.Single();

        result.ShouldNotHaveAException();
        result.ShouldNotHaveDiagnostics();
        result.ShouldHaveGeneratedSource("MyBuilderBase.g.cs", expectedSourceCode);
    }

    [Test]
    public void YashSourceGenerator_GeneratesPartialClass_WhenBuilderClassHasYashAttribute()
    {
        // lang=c#
        var expectedSourceCode = $@"
            // <auto-generated />
            #nullable enable

            namespace MyNamespace;

            {Constants.GeneratedCodeAttribute}
            public partial class MyBuilder : MyBuilderBase<MyBuilder> {{

                protected override MyBuilder Instance => this;

                public MyClass Build(){{
                    return BuildInternal(justAStringInternal ?? DefaultValues.justAString);
                }}

            }};
        ";

        var result = GeneratorTestHelper.RunGenerator<YashSourceGenerator>(SourceCode).Results.Single();

        result.ShouldNotHaveAException();
        result.ShouldNotHaveDiagnostics();
        result.ShouldHaveGeneratedSource("MyBuilder.g.cs", expectedSourceCode);
    }

    [Test]
    public void YashSourceGenerator_UsesCachedSources_WhenRelevantSourceIsNotModified()
    {
        var modifiedSourceCodeForSecondRun = $@"
            // This is a comment to re-trigger generation
            {SourceCode}
        ";

        var result = GeneratorTestHelper.RunGenerator<YashSourceGenerator>(
                SourceCode,
                withTracking: true,
                modifiedSourceCodeForSecondRun)
            .Results.Single();

        result.ShouldNotHaveAException();
        result.ShouldNotHaveDiagnostics();
        result.ShouldHaveAllTrackedOutputStepsReasonsBeCached();
    }
}