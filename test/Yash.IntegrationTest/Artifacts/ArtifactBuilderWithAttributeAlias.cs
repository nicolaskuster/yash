namespace Yash.IntegrationTest.Artifacts;

using AttributeAlias = Yash.Generated.Attributes.YashAttribute<Yash.IntegrationTest.Artifacts.Artifact>;

[AttributeAlias]
public partial class ArtifactBuilderWithAttributeAlias : IArtifactBuilder
{
    protected override DefaultValuesRecord DefaultValues => new("aName", new ArtifactItem(1));

    // Only needed to satisfy the IArtifactBuilder Interface
    public new IArtifactBuilder SetName(string value) => base.SetName(value);

    // Only needed to satisfy the IArtifactBuilder Interface
    public new IArtifactBuilder SetItem(ArtifactItem value) => base.SetItem(value);

    private static Artifact BuildInternal(string name, ArtifactItem item) => new(name, item);
}