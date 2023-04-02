namespace Yash.IntegrationTest;

using System.Collections;
using FluentAssertions;
using NUnit.Framework;
using Yash.IntegrationTest.Artifacts;

public class YashAttributeTest
{
    [TestCaseSource(nameof(GetArtifactBuilderTestCases))]
    public void ArtifactBuilder_UsesDefaultValues_WhenNothingIsSetExplicit(IArtifactBuilder builder)
    {
        var artifact = builder.Build();

        artifact.Name.Should().Be("aName");
        artifact.Item.Value.Should().Be(1);
    }

    [TestCaseSource(nameof(GetArtifactBuilderTestCases))]
    public void ArtifactBuilder_UsesExplicitValue_WhenNameIsSet(IArtifactBuilder builder)
    {
        var artifact = builder
            .SetName("anotherName")
            .Build();

        artifact.Name.Should().Be("anotherName");
        artifact.Item.Value.Should().Be(1);
    }

    [TestCaseSource(nameof(GetArtifactBuilderTestCases))]
    public void ArtifactBuilder_UsesExplicitValue_WhenItemIsSet(IArtifactBuilder builder)
    {
        var artifact = builder
            .SetItem(new ArtifactItem(2))
            .Build();

        artifact.Name.Should().Be("aName");
        artifact.Item.Value.Should().Be(2);
    }

    [TestCaseSource(nameof(GetArtifactBuilderTestCases))]
    public void ArtifactBuilder_UsesExplicitValue_WhenNameAndItemIsSet(IArtifactBuilder builder)
    {
        var artifact = builder
            .SetName("anotherName")
            .SetItem(new ArtifactItem(2))
            .Build();

        artifact.Name.Should().Be("anotherName");
        artifact.Item.Value.Should().Be(2);
    }

    private static IEnumerable GetArtifactBuilderTestCases()
    {
        yield return new TestCaseData(new ArtifactBuilder()).SetName(nameof(ArtifactBuilder));
        yield return new TestCaseData(new ArtifactBuilderWithAttributeAlias()).SetName(nameof(ArtifactBuilderWithAttributeAlias));
        yield return new TestCaseData(new ArtifactBuilderWithTargetTypeAlias()).SetName(nameof(ArtifactBuilderWithTargetTypeAlias));
    }
}