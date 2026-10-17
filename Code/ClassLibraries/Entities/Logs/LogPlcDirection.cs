namespace Gudel.GLogWare.Entities;

[SeedOrder(1)]
public class LogPlcDirection : BaseTracking, ISeedData<LogPlcDirection>
{
    public string? Identifier { get; set; }
    public string? TranslationKey { get; set; }
    public string? Description { get; set; }

    public ICollection<LogPlc> LogPlcs { get; set; } = [];

    public static IEnumerable<LogPlcDirection> SeedData()
    {
        return [
            new() { 
                Identifier = nameof(LogPlcDirectionIdentifiers.GENERAL), 
                TranslationKey = $"{nameof(LogPlcDirection)}.{nameof(LogPlcDirectionIdentifiers.GENERAL)}",
                Description = "General information"
            },
            new() {
                Identifier = nameof(LogPlcDirectionIdentifiers.GLOGWARE_TO_PLC),
                TranslationKey = $"{nameof(LogPlcDirection)}.{nameof(LogPlcDirectionIdentifiers.GLOGWARE_TO_PLC)}",
                Description = "GLogWare ==> PLC"
            },
            new() {
                Identifier = nameof(LogPlcDirectionIdentifiers.PLC_TO_GLOGWARE),
                TranslationKey = $"{nameof(LogPlcDirection)}.{nameof(LogPlcDirectionIdentifiers.PLC_TO_GLOGWARE)}",
                Description = "PLC ==> GLogWare"
            }
        ];
    }
}
