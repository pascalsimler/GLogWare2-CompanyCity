namespace Gudel.GLogWare.EFCore.Domain;

[SeedOrder(1)]
public class LogPlcCategory : BaseTracking, ISeedData<LogPlcCategory>
{
    public string? Name { get; set; }
    public string? TranslationKey { get; set; }
    public string? Description { get; set; }

    public ICollection<LogPlc> LogPlcs { get; set; } = new List<LogPlc>();

    public static IEnumerable<LogPlcCategory> SeedData()
    {
        return new List<LogPlcCategory>() {
            new LogPlcCategory { 
                Name = LogPlcCategoryIdentifiers.CONVEYOR.ToString(),
                TranslationKey = $"{typeof(LogPlcCategory).Name}.{LogPlcCategoryIdentifiers.CONVEYOR.ToString()}",
                Description = "Conveyor"
            },
            new LogPlcCategory {
                Name = LogPlcCategoryIdentifiers.GANTRY.ToString(),
                TranslationKey = $"{typeof(LogPlcCategory).Name}.{LogPlcCategoryIdentifiers.GANTRY.ToString()}",
                Description = "Gantry FP"
            },
            new LogPlcCategory {
                Name = LogPlcCategoryIdentifiers.PALLETIZER.ToString(),
                TranslationKey = $"{typeof(LogPlcCategory).Name}.{LogPlcCategoryIdentifiers.PALLETIZER.ToString()}",
                Description = "Palletizer ZP"
            },
            new LogPlcCategory {
                Name = LogPlcCategoryIdentifiers.SHUTTLE.ToString(),
                TranslationKey = $"{typeof(LogPlcCategory).Name}.{LogPlcCategoryIdentifiers.SHUTTLE.ToString()}",
                Description = "Powertrain shuttle"
            },
            new LogPlcCategory {
                Name = LogPlcCategoryIdentifiers.KUKA_ROBOT.ToString(),
                TranslationKey = $"{typeof(LogPlcCategory).Name}.{LogPlcCategoryIdentifiers.KUKA_ROBOT.ToString()}",
                Description = "KUKA Robot"
            },
            new LogPlcCategory {
                Name = LogPlcCategoryIdentifiers.UNCATEGORIZED.ToString(),
                TranslationKey = $"{typeof(LogPlcCategory).Name}.{LogPlcCategoryIdentifiers.UNCATEGORIZED.ToString()}",
                Description = "Uncategorized"
            },
        };
    }
}
