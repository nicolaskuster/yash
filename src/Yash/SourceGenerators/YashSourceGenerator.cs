namespace Yash.SourceGenerators;

using System.Linq;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Yash.CodeGeneration;

[Generator(LanguageNames.CSharp)]
public class YashSourceGenerator : IIncrementalGenerator
{
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        context.RegisterPostInitializationOutput(PostInitializationOutput);

        var candidatesPipeline = context.SyntaxProvider.CreateSyntaxProvider(
                SyntaxPredicate,
                SyntaxTransform)
            .Where(candidate => candidate is not null)
            .Collect()
            .SelectMany((candidates, _) => candidates.Distinct().Select(candidate => (YashBuilderContext)candidate!));

        context.RegisterSourceOutput(candidatesPipeline, Execute);
    }

    private static void Execute(SourceProductionContext context, YashBuilderContext yashBuilderContext)
    {
        var generatedSources = YashBuilderGenerator.Generate(yashBuilderContext);

        foreach (var (hintName, sourceText) in generatedSources)
        {
            context.AddSource(hintName, sourceText);
        }
    }

    private static bool SyntaxPredicate(SyntaxNode node, CancellationToken cancellationToken)
    {
        return node is ClassDeclarationSyntax candidate
               && candidate.AttributeLists.Count > 0
               && candidate.Modifiers.Any(SyntaxKind.PartialKeyword)
               && !candidate.Modifiers.Any(SyntaxKind.StaticKeyword);
    }

    private static YashBuilderContext? SyntaxTransform(
        GeneratorSyntaxContext context,
        CancellationToken cancellationToken)
    {
        var classDeclarationSyntax = (ClassDeclarationSyntax)context.Node;
        var semanticModel = context.SemanticModel;

        var yashClassInfoProvider = YashClassInfoProvider.TryCreate(
            classDeclarationSyntax,
            semanticModel,
            cancellationToken);

        if (yashClassInfoProvider is null)
        {
            return null;
        }

        var properties = yashClassInfoProvider.InternalBuildMethodParameters
            .Select(p => new Property(p.Type.ToDisplayString(), p.Name));

        return new YashBuilderContext(
            @namespace: yashClassInfoProvider.ClassSymbol.ContainingNamespace.IsGlobalNamespace
                ? string.Empty
                : yashClassInfoProvider.ClassSymbol.ContainingNamespace.ToDisplayString(),
            builderClassName: yashClassInfoProvider.ClassSymbol.Name,
            builderAccessibility: yashClassInfoProvider.ClassSymbol.DeclaredAccessibility.ToString().ToLowerInvariant(),
            targetClassName: yashClassInfoProvider.TargetType.Name,
            properties: properties.ToArray());
    }

    private static void PostInitializationOutput(IncrementalGeneratorPostInitializationContext context)
    {
        var (attributeHintName, attributeSourceText) = YashAttributeGenerator.Generate();
        context.AddSource(attributeHintName, attributeSourceText);
    }
}