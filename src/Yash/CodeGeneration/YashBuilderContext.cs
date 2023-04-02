namespace Yash.CodeGeneration;

using System;
using System.Linq;

public readonly struct YashBuilderContext : IEquatable<YashBuilderContext>
{
    public YashBuilderContext(
        string? @namespace,
        string builderClassName,
        string builderAccessibility,
        string targetClassName,
        Property[] properties)
    {
        Namespace = @namespace;
        BuilderClassName = builderClassName;
        BuilderAccessibility = builderAccessibility;
        TargetClassName = targetClassName;
        Properties = properties;
    }

    public string? Namespace { get; }

    public string BuilderClassName { get; }

    public string BuilderAccessibility { get; }

    public string TargetClassName { get; }

    public Property[] Properties { get; }

    public static bool operator ==(YashBuilderContext left, YashBuilderContext right)
    {
        return left.Equals(right);
    }

    public static bool operator !=(YashBuilderContext left, YashBuilderContext right)
    {
        return !left.Equals(right);
    }

    public bool Equals(YashBuilderContext other)
    {
        return Namespace == other.Namespace
               && BuilderClassName == other.BuilderClassName
               && BuilderAccessibility == other.BuilderAccessibility
               && TargetClassName == other.TargetClassName
               && Properties.SequenceEqual(other.Properties);
    }

    public override bool Equals(object? obj)
    {
        return obj is YashBuilderContext other && Equals(other);
    }

    public override int GetHashCode()
    {
        unchecked
        {
            var hashCode = Namespace?.GetHashCode() ?? 0;
            hashCode = (hashCode * 397) ^ BuilderClassName.GetHashCode();
            hashCode = (hashCode * 397) ^ BuilderAccessibility.GetHashCode();
            hashCode = (hashCode * 397) ^ TargetClassName.GetHashCode();
            hashCode = (hashCode * 397) ^ Properties.GetHashCode();
            return hashCode;
        }
    }
}