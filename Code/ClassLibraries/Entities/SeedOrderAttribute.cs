namespace Gudel.GLogWare.Entities;

[AttributeUsage(AttributeTargets.Class)]
public sealed class SeedOrderAttribute(int order) : Attribute
{
    public int Order { get; } = order;
}