namespace Yash.IntegrationTest.Artifacts;

public interface IArtifactBuilder
{
    public Artifact Build();

    IArtifactBuilder SetName(string value);

    IArtifactBuilder SetItem(ArtifactItem value);
}