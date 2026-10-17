namespace Gudel.GLogWare.Entities;

public class Article : BaseTracking
{
    public string ArticleNumber { get; set; } = null!;
    public string? Description { get; set; }
    public string? Remarks { get; set; }

    public ICollection<Sku> Skus { get; set; } = [];
}