namespace Gudel.GLogWare.Entities;

[SeedOrder(1)]
public class LogPlcCategory : BaseTracking, ISeedData<LogPlcCategory>
{
    public string? Identifier { get; set; }
    public string? TranslationKey { get; set; }
    public string? Description { get; set; }

    public ICollection<LogPlc> LogPlcs { get; set; } = [];

    public static IEnumerable<LogPlcCategory> SeedData()
    {
        return [
            new() { 
                Identifier = nameof(LogPlcCategoryIdentifiers.CONVEYOR),
                TranslationKey = $"{nameof(LogPlcCategory)}.{nameof(LogPlcCategoryIdentifiers.CONVEYOR)}",
                Description = "Conveyor"
            },
            new() {
                Identifier = nameof(LogPlcCategoryIdentifiers.GANTRY),
                TranslationKey = $"{nameof(LogPlcCategory)}.{nameof(LogPlcCategoryIdentifiers.GANTRY)}",
                Description = "Gantry FP"
            },
            new() {
                Identifier = nameof(LogPlcCategoryIdentifiers.PALLETIZER),
                TranslationKey = $"{nameof(LogPlcCategory)}.{nameof(LogPlcCategoryIdentifiers.PALLETIZER)}",
                Description = "Palletizer ZP"
            },
            new() {
                Identifier = nameof(LogPlcCategoryIdentifiers.SHUTTLE),
                TranslationKey = $"{nameof(LogPlcCategory)}.{nameof(LogPlcCategoryIdentifiers.SHUTTLE)}",
                Description = "Powertrain shuttle"
            },
            new() {
                Identifier = nameof(LogPlcCategoryIdentifiers.KUKA_ROBOT),
                TranslationKey = $"{nameof(LogPlcCategory)}.{nameof(LogPlcCategoryIdentifiers.KUKA_ROBOT)}",
                Description = "KUKA Robot"
            },
            new() {
                Identifier = nameof(LogPlcCategoryIdentifiers.UNCATEGORIZED),
                TranslationKey = $"{nameof(LogPlcCategory)}.{nameof(LogPlcCategoryIdentifiers.UNCATEGORIZED)}",
                Description = "Uncategorized"
            },
        ];
    }
}
