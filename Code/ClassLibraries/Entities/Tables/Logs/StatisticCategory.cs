namespace Gudel.GLogWare.Entities;

[SeedOrder(1)]
public class StatisticCategory : ISeedData<StatisticCategory>
{
    public string? Identifier { get; set; }
    public string? TranslationKey { get; set; }
    public string? Description { get; set; }

    public ICollection<Statistic> Statistics { get; set; } = [];

    public static IEnumerable<StatisticCategory> SeedData()
    {
        return [
            new() {
                Identifier = nameof(StatisticCategoryIdentifiers.Bridge),
                TranslationKey = $"{nameof(LogPlcDirection)}.{nameof(StatisticCategoryIdentifiers.Bridge)}",
                Description = "Bridge movements statistics"
            },
            new() {
                Identifier = nameof(StatisticCategoryIdentifiers.FillLevel),
                TranslationKey = $"{nameof(LogPlcDirection)}.{nameof(StatisticCategoryIdentifiers.FillLevel)}",
                Description = "Gantry Fill level statistics"
            },
        ];
    }
}
