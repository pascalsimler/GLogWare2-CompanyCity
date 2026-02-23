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
                Name = LogPlcCategoryNames.CONVEYOR.ToString(),
                TranslationKey = $"{typeof(LogPlcCategory).Name}.{LogPlcCategoryNames.CONVEYOR.ToString()}",
                Description = "Conveyor"
            },
            new LogPlcCategory {
                Name = LogPlcCategoryNames.GANTRY.ToString(),
                TranslationKey = $"{typeof(LogPlcCategory).Name}.{LogPlcCategoryNames.GANTRY.ToString()}",
                Description = "Gantry FP"
            },
            new LogPlcCategory {
                Name = LogPlcCategoryNames.PALLETIZER.ToString(),
                TranslationKey = $"{typeof(LogPlcCategory).Name}.{LogPlcCategoryNames.PALLETIZER.ToString()}",
                Description = "Palletizer ZP"
            },
            new LogPlcCategory {
                Name = LogPlcCategoryNames.SHUTTLE.ToString(),
                TranslationKey = $"{typeof(LogPlcCategory).Name}.{LogPlcCategoryNames.SHUTTLE.ToString()}",
                Description = "Powertrain shuttle"
            },
            new LogPlcCategory {
                Name = LogPlcCategoryNames.KUKA_ROBOT.ToString(),
                TranslationKey = $"{typeof(LogPlcCategory).Name}.{LogPlcCategoryNames.KUKA_ROBOT.ToString()}",
                Description = "KUKA Robot"
            },
            new LogPlcCategory {
                Name = LogPlcCategoryNames.UNCATEGORIZED.ToString(),
                TranslationKey = $"{typeof(LogPlcCategory).Name}.{LogPlcCategoryNames.UNCATEGORIZED.ToString()}",
                Description = "Uncategorized"
            },
        };
    }
}
