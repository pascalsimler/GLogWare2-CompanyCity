namespace Gudel.GLogWare.Entities;

[AttributeUsage(AttributeTargets.Class)]
public sealed class SeedOrderAttribute : Attribute
{
    public int Order { get; }
    public SeedOrderAttribute(int order) => Order = order;
}