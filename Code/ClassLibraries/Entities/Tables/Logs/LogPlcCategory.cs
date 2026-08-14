namespace Gudel.GLogWare.Entities;

[SeedOrder(1)]
public class LogPlcCategory : BaseTracking, ISeedData<LogPlcCategory>
{
    public string? Identifier { get; set; }
    public string? TranslationKey { get; set; }
    public string? Description { get; set; }

    public ICollection<LogPlc> LogPlcs { get; set; } = new List<LogPlc>();

    public static IEnumerable<LogPlcCategory> SeedData()
    {
        return new List<LogPlcCategory>() {
            new LogPlcCategory { 
                Identifier = nameof(LogPlcCategoryIdentifiers.CONVEYOR),
                TranslationKey = $"{nameof(LogPlcCategory)}.{nameof(LogPlcCategoryIdentifiers.CONVEYOR)}",
                Description = "Conveyor"
            },
            new LogPlcCategory {
                Identifier = nameof(LogPlcCategoryIdentifiers.GANTRY),
                TranslationKey = $"{nameof(LogPlcCategory)}.{nameof(LogPlcCategoryIdentifiers.GANTRY)}",
                Description = "Gantry FP"
            },
            new LogPlcCategory {
                Identifier = nameof(LogPlcCategoryIdentifiers.PALLETIZER),
                TranslationKey = $"{nameof(LogPlcCategory)}.{nameof(LogPlcCategoryIdentifiers.PALLETIZER)}",
                Description = "Palletizer ZP"
            },
            new LogPlcCategory {
                Identifier = nameof(LogPlcCategoryIdentifiers.SHUTTLE),
                TranslationKey = $"{nameof(LogPlcCategory)}.{nameof(LogPlcCategoryIdentifiers.SHUTTLE)}",
                Description = "Powertrain shuttle"
            },
            new LogPlcCategory {
                Identifier = nameof(LogPlcCategoryIdentifiers.KUKA_ROBOT),
                TranslationKey = $"{nameof(LogPlcCategory)}.{nameof(LogPlcCategoryIdentifiers.KUKA_ROBOT)}",
                Description = "KUKA Robot"
            },
            new LogPlcCategory {
                Identifier = nameof(LogPlcCategoryIdentifiers.UNCATEGORIZED),
                TranslationKey = $"{nameof(LogPlcCategory)}.{nameof(LogPlcCategoryIdentifiers.UNCATEGORIZED)}",
                Description = "Uncategorized"
            },
        };
    }
}
