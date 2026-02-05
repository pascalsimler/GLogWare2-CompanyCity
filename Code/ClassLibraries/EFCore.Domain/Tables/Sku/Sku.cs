namespace Gudel.GLogWare.EFCore.Domain;

public class Sku : BaseTracking
{
    public string SkuId { get; set; } = null!;
    public string Article { get; set; } = null!;
    public string? JobId { get; set; } = null!;
    public string Place { get; set; } = null!;
    public int PositionInStack { get; set; }

    public Job JobRecord { get; set; } = null!;
    public Article ArticleRecord { get; set; } = null!;
    public Place PlaceRecord { get; set; } = null!;
}
