namespace Yash;

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

public class YashClassInfoProvider
{
    private readonly ClassDeclarationSyntax yashClassDeclaration;
    private readonly IMethodSymbol yashAttribute;
    private readonly SemanticModel semanticModel;
    private readonly CancellationToken cancellationToken;

    private YashClassInfoProvider(
        ClassDeclarationSyntax yashClassDeclaration,
        IMethodSymbol yashAttribute,
        SemanticModel semanticModel,
        CancellationToken cancellationToken)
    {
        this.yashClassDeclaration = yashClassDeclaration;
        this.yashAttribute = yashAttribute;
        this.semanticModel = semanticModel;
        this.cancellationToken = cancellationToken;
    }

    public ISymbol ClassSymbol => LazyClassSymbol.Value;

    public ITypeSymbol TargetType => yashAttribute.ContainingType.TypeArguments[0];

    public ImmutableArray<MethodDeclarationSyntax> InternalBuildMethods => LazyInternalBuildMethods.Value;

    public ImmutableArray<IParameterSymbol> InternalBuildMethodParameters => LazyInternalBuildMethodParameters.Value;

    private Lazy<ISymbol> LazyClassSymbol =>
        new(() => semanticModel.GetDeclaredSymbol(yashClassDeclaration, cancellationToken)!);

    private Lazy<ImmutableArray<MethodDeclarationSyntax>> LazyInternalBuildMethods =>
        new(GetInternalBuildMethods);

    private Lazy<ImmutableArray<IParameterSymbol>> LazyInternalBuildMethodParameters =>
        new(GetInternalBuildMethodParameters);

    public static YashClassInfoProvider? TryCreate(
        ClassDeclarationSyntax classDeclaration,
        SemanticModel semanticModel,
        CancellationToken cancellationToken)
    {
        if (classDeclaration.AttributeLists.Count == 0)
        {
            return null;
        }

        var attribute = TryGetYashAttribute(classDeclaration, semanticModel, cancellationToken);

        if (attribute is null)
        {
            return null;
        }

        return new YashClassInfoProvider(classDeclaration, attribute, semanticModel, cancellationToken);
    }

    private static IMethodSymbol? TryGetYashAttribute(
        ClassDeclarationSyntax classDeclaration,
        SemanticModel semanticModel,
        CancellationToken cancellationToken)
    {
        foreach (var attributeList in classDeclaration.AttributeLists)
        {
            ThrowIfCancellationIsRequested(cancellationToken);

            foreach (var attribute in attributeList.Attributes)
            {
                ThrowIfCancellationIsRequested(cancellationToken);

                if (semanticModel.GetSymbolInfo(attribute, cancellationToken).Symbol
                    is not IMethodSymbol attributeSymbol)
                {
                    continue;
                }

                var displayFormat = new SymbolDisplayFormat(
                    globalNamespaceStyle: SymbolDisplayGlobalNamespaceStyle.Omitted,
                    typeQualificationStyle: SymbolDisplayTypeQualificationStyle.NameAndContainingTypesAndNamespaces,
                    genericsOptions: SymbolDisplayGenericsOptions.None);

                var fullName = attributeSymbol.ContainingType.ToDisplayString(displayFormat);
                if (fullName == Constants.AttributeFullName)
                {
                    return attributeSymbol;
                }
            }
        }

        return null;
    }

    private static void ThrowIfCancellationIsRequested(CancellationToken cancellationToken)
    {
        if (cancellationToken.IsCancellationRequested)
        {
            throw new OperationCanceledException(cancellationToken);
        }
    }

    private ImmutableArray<MethodDeclarationSyntax> GetInternalBuildMethods()
    {
        var methodDeclarations = new List<MethodDeclarationSyntax>();
        foreach (var memberDeclaration in yashClassDeclaration.Members)
        {
            ThrowIfCancellationIsRequested(cancellationToken);

            if (memberDeclaration is not MethodDeclarationSyntax methodDeclaration)
            {
                continue;
            }

            if (methodDeclaration.Identifier.ValueText != Constants.InternalBuildMethodName)
            {
                continue;
            }

            var returnType = semanticModel.GetTypeInfo(methodDeclaration.ReturnType, cancellationToken).Type;
            if (!SymbolEqualityComparer.Default.Equals(returnType, TargetType))
            {
                continue;
            }

            methodDeclarations.Add(methodDeclaration);
        }

        return methodDeclarations.ToImmutableArray();
    }

    private ImmutableArray<IParameterSymbol> GetInternalBuildMethodParameters()
    {
        if (InternalBuildMethods.Length < 1)
        {
            return ImmutableArray<IParameterSymbol>.Empty;
        }

        var methodSymbol = semanticModel.GetDeclaredSymbol(InternalBuildMethods[0], cancellationToken) as IMethodSymbol;

        if (methodSymbol is null)
        {
            return ImmutableArray<IParameterSymbol>.Empty;
        }

        return methodSymbol.Parameters.ToImmutableArray();
    }
}