namespace Yash.IntegrationTest.Artifacts;

using Yash.Generated.Attributes;

[Yash<Artifact>]
public partial class ArtifactBuilder : IArtifactBuilder
{
    protected override DefaultValuesRecord DefaultValues => new("aName", new ArtifactItem(1));

    // Only needed to satisfy the IArtifactBuilder Interface
    public new IArtifactBuilder SetName(string value) => base.SetName(value);

    // Only needed to satisfy the IArtifactBuilder Interface
    public new IArtifactBuilder SetItem(ArtifactItem value) => base.SetItem(value);

    private static Artifact BuildInternal(string name, ArtifactItem item) => new(name, item);
}