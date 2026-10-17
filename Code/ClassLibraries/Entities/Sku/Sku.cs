namespace Gudel.GLogWare.Entities;

public class Sku : BaseTracking
{
    public string Skuid { get; set; } = null!;
    public string? Jobid { get; set; } = null!;
    public string? SkuType { get; set; } = null!;
    public string? Article { get; set; } = null!;
    public string? Place { get; set; } = null!;
    public int? PositionInStack { get; set; }

    public Job JobRecord { get; set; } = null!;
    public SkuType SkuTypeRecord { get; set; } = null!;
    public Article ArticleRecord { get; set; } = null!;
    public Place PlaceRecord { get; set; } = null!;
}
