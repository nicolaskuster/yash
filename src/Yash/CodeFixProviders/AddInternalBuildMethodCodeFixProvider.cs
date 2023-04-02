namespace Yash.CodeFixProviders;

using System.Collections.Immutable;
using System.Composition;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

[Shared]
[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(AddInternalBuildMethodCodeFixProvider))]
public class AddInternalBuildMethodCodeFixProvider : CodeFixProvider
{
    public override ImmutableArray<string> FixableDiagnosticIds =>
        ImmutableArray.Create(Constants.YashClassWithMissingInternalBuildMethodDiagnosticId);

    public sealed override FixAllProvider GetFixAllProvider() => WellKnownFixAllProviders.BatchFixer;

    public override async Task RegisterCodeFixesAsync(CodeFixContext context)
    {
        var cancellationToken = context.CancellationToken;
        var root = await context.Document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);

        var diagnostic = context.Diagnostics[0];
        var diagnosticSpan = diagnostic.Location.SourceSpan;

        var classDeclaration = root?
            .FindToken(diagnosticSpan.Start)
            .Parent?
            .AncestorsAndSelf()
            .OfType<ClassDeclarationSyntax>()
            .FirstOrDefault();

        if (classDeclaration is null)
        {
            return;
        }

        var semanticModel = await context.Document.GetSemanticModelAsync(cancellationToken).ConfigureAwait(false);
        var yashClassInfoProvider = YashClassInfoProvider.TryCreate(
            classDeclaration,
            semanticModel!,
            cancellationToken);

        if (yashClassInfoProvider is null)
        {
            return;
        }

        var targetType = yashClassInfoProvider.TargetType;

        context.RegisterCodeFix(
            CodeAction.Create(
                $"Add 'private static {targetType.ToDisplayString()} {Constants.InternalBuildMethodName}();'",
                createChangedDocument: ct =>
                    AddInternalBuildMethodAsync(context.Document, classDeclaration, targetType, ct),
                nameof(AddInternalBuildMethodCodeFixProvider)),
            diagnostic);
    }

    private static async Task<Document> AddInternalBuildMethodAsync(
        Document document,
        ClassDeclarationSyntax classDeclaration,
        ISymbol targetType,
        CancellationToken cancellationToken)
    {
        var internalBuildMethodDeclaration = SyntaxFactory
            .ParseMemberDeclaration(
                $"private static {targetType.ToDisplayString()} {Constants.InternalBuildMethodName}() => throw new System.NotImplementedException();")
            !.WithTrailingTrivia(SyntaxFactory.LineFeed);

        var members = classDeclaration.Members.Add(internalBuildMethodDeclaration);

        var newClassDeclaration = classDeclaration.WithMembers(members);

        var root = await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);
        var newRoot = root!.ReplaceNode(classDeclaration, newClassDeclaration)!;

        return document.WithSyntaxRoot(newRoot);
    }
}