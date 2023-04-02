#pragma warning disable SA1311 // StaticReadonlyFieldsMustBeginWithUpperCaseLetter
namespace Yash.Analyzers;

using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class YashAnalyzer : DiagnosticAnalyzer
{
    // @formatter:off
    private static readonly DiagnosticDescriptor YashClassNeedsToBePartialRule = new(
        id: Constants.YashClassNeedsToBePartialDiagnosticId,
        title: "Class with Yash attribute needs to be partial",
        messageFormat: "Make class '{0}' partial to allow Yash to generate code",
        category: "Usage",
        defaultSeverity: DiagnosticSeverity.Warning,
        isEnabledByDefault: true);

    private static readonly DiagnosticDescriptor YashClassCantBeStaticRule = new(
        id: Constants.YashClassCantBeStaticDiagnosticId,
        title: "Class with Yash attribute can't be static",
        messageFormat: "Make class '{0}' non-static to allow Yash to generate code",
        category: "Usage",
        defaultSeverity: DiagnosticSeverity.Warning,
        isEnabledByDefault: true);

    private static readonly DiagnosticDescriptor YashClassMissingInternalBuildMethodRule = new(
        id: Constants.YashClassWithMissingInternalBuildMethodDiagnosticId,
        title: $"Missing required method '{Constants.InternalBuildMethodName}()' in class with '{Constants.AttributeName}' attribute",
        messageFormat: $"Add 'private static {0} {Constants.InternalBuildMethodName}();' to {1}",
        category: "Usage",
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    private static readonly DiagnosticDescriptor YashClassMultipleInternalBuildMethodsRule = new(
        id: Constants.YashClassMultipleInternalBuildMethodsDiagnosticId,
        title: $"Internal build method ({Constants.InternalBuildMethodName}();) is defined multiple times",
        messageFormat: $"Ensure there is only one definition of the '{Constants.InternalBuildMethodName}()' method in {0}",
        category: "Usage",
        defaultSeverity: DiagnosticSeverity.Warning,
        isEnabledByDefault: true);

    // @formatter:on
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
        ImmutableArray.Create(
            YashClassNeedsToBePartialRule,
            YashClassCantBeStaticRule,
            YashClassMissingInternalBuildMethodRule,
            YashClassMultipleInternalBuildMethodsRule);

    public override void Initialize(AnalysisContext context)
    {
        context.EnableConcurrentExecution();
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);

        context.RegisterSyntaxNodeAction(Analyze, SyntaxKind.ClassDeclaration);
    }

    private static void Analyze(SyntaxNodeAnalysisContext context)
    {
        var classDeclaration = (ClassDeclarationSyntax)context.Node;

        var yashClassInfoProvider = YashClassInfoProvider.TryCreate(
            classDeclaration,
            context.SemanticModel,
            context.CancellationToken);

        if (yashClassInfoProvider is null)
        {
            return;
        }

        if (!classDeclaration.Modifiers.Any(SyntaxKind.PartialKeyword))
        {
            context.ReportDiagnostic(Diagnostic.Create(
                YashClassNeedsToBePartialRule,
                classDeclaration.Keyword.GetLocation(),
                classDeclaration.Identifier.ValueText));
        }

        if (classDeclaration.Modifiers.Any(SyntaxKind.StaticKeyword))
        {
            context.ReportDiagnostic(Diagnostic.Create(
                YashClassCantBeStaticRule,
                classDeclaration.Modifiers.First(m => m.IsKind(SyntaxKind.StaticKeyword))
                    .GetLocation(),
                classDeclaration.Identifier.ValueText));
        }

        var internalBuildMethods = yashClassInfoProvider.InternalBuildMethods;

        if (internalBuildMethods.Length == 0)
        {
            context.ReportDiagnostic(Diagnostic.Create(
                YashClassMissingInternalBuildMethodRule,
                classDeclaration.Identifier.GetLocation(),
                yashClassInfoProvider.TargetType.ToDisplayString(),
                classDeclaration.Identifier.ValueText));
        }

        if (internalBuildMethods.Length > 1)
        {
            foreach (var internalBuildMethod in internalBuildMethods)
            {
                context.ReportDiagnostic(Diagnostic.Create(
                    YashClassMultipleInternalBuildMethodsRule,
                    internalBuildMethod.Identifier.GetLocation(),
                    classDeclaration.Identifier.ValueText));
            }
        }
    }
}