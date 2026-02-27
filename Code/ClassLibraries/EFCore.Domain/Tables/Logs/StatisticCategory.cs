namespace Gudel.GLogWare.EFCore.Domain;

[SeedOrder(1)]
public class StatisticCategory : ISeedData<StatisticCategory>
{
    public string? Identifier { get; set; }
    public string? TranslationKey { get; set; }
    public string? Description { get; set; }

    public ICollection<Statistic> Statistics { get; set; } = new List<Statistic>();

    public static IEnumerable<StatisticCategory> SeedData()
    {
        return new List<StatisticCategory>() {
            new StatisticCategory {
                Identifier = nameof(StatisticCategoryIdentifiers.Bridge),
                TranslationKey = $"{nameof(LogPlcDirection)}.{nameof(StatisticCategoryIdentifiers.Bridge)}",
                Description = "Bridge movements statistics"
            },
            new StatisticCategory {
                Identifier = nameof(StatisticCategoryIdentifiers.FillLevel),
                TranslationKey = $"{nameof(LogPlcDirection)}.{nameof(StatisticCategoryIdentifiers.FillLevel)}",
                Description = "Gantry Fill level statistics"
            },
        };
    }
}
