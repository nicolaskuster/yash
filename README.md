# Yash
A C# source generator to generate builder classes for your types.

## Getting started
Reference the `Yash` NuGet package - that's it.

### Prerequisites

The Yash package targets .NET Standard 2.0 for host flexibility. Note that Yash looks for, and generates, code that targets .NET 7.

## Usage
Assuming you have the following types in your project:
```csharp
public record Artifact(string Name, ArtifactItem Item);
public record ArtifactItem(int Value);
```

To generate a builder class for the `Artifact`, you can create a partial class, apply the `[Yash<Artifact>]` attribute, and implement the `BuildInternal()` method in your project:
```csharp
[Yash<Artifact>]
public partial class ArtifactBuilder {
    protected override DefaultValuesRecord DefaultValues => new("aName", new ArtifactItem(1));

    private Artifact BuildInternal(string name, ArtifactItem item) => new Artifact(name, item);
}
```

Based on the `Buildinternal()` method, Yash will generate all the required build methods and forces you to override the `DefaultValues` property.<br/>
The Builder can then be used like this:
```csharp
var artifact = new ArtifactBuilder()
    .SetName("anotherName")
    .SetItem(new ArtifactItem(12))
    .Build();
```

## License
Yash is licensed under the MIT license. See [LICENSE](https://github.com/nicolaskuster/yash/blob/master/LICENSE) for details.