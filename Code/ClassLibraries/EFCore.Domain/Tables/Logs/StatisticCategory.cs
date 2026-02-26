namespace Gudel.GLogWare.EFCore.Domain;

public class StatisticCategory
{
    public string? Identifier { get; set; }
    public string? TranslationKey { get; set; }
    public string? Description { get; set; }

    public ICollection<Statistic> Statistics { get; set; } = new List<Statistic>();
}
