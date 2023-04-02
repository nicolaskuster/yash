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
[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(MakeClassPartialCodeFixProvider))]
public class MakeClassPartialCodeFixProvider : CodeFixProvider
{
    public override ImmutableArray<string> FixableDiagnosticIds =>
        ImmutableArray.Create(Constants.YashClassNeedsToBePartialDiagnosticId);

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

        context.RegisterCodeFix(
            CodeAction.Create(
                "Make class partial",
                createChangedDocument: ct => MakeClassPartialAsync(context.Document, classDeclaration, ct),
                nameof(MakeClassPartialCodeFixProvider)),
            diagnostic);
    }

    private static async Task<Document> MakeClassPartialAsync(
        Document document,
        ClassDeclarationSyntax classDeclaration,
        CancellationToken cancellationToken)
    {
        var modifiers = classDeclaration.Modifiers.Add(SyntaxFactory.Token(SyntaxKind.PartialKeyword));

        var newClassDeclaration = classDeclaration.WithModifiers(modifiers);

        var root = await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);
        var newRoot = root!.ReplaceNode(classDeclaration, newClassDeclaration)!;

        return document.WithSyntaxRoot(newRoot);
    }
}