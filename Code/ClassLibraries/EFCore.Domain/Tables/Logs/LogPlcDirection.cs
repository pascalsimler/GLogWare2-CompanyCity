namespace Gudel.GLogWare.EFCore.Domain;

[SeedOrder(1)]
public class LogPlcDirection : BaseTracking, ISeedData<LogPlcDirection>
{
    public string? Name { get; set; }
    public string? TranslationKey { get; set; }
    public string? Description { get; set; }

    public ICollection<LogPlc> LogPlcs { get; set; } = new List<LogPlc>();

    public static IEnumerable<LogPlcDirection> SeedData()
    {
        return new List<LogPlcDirection>() {
            new LogPlcDirection { 
                Name = LogPlcDirectionNames.GENERAL.ToString(), 
                TranslationKey = $"{typeof(LogPlcDirection).Name}.{LogPlcDirectionNames.GENERAL.ToString()}",
                Description = "General information"
            },
            new LogPlcDirection {
                Name = LogPlcDirectionNames.GLOGWARE_TO_PLC.ToString(),
                TranslationKey = $"{typeof(LogPlcDirection).Name}.{LogPlcDirectionNames.GLOGWARE_TO_PLC.ToString()}",
                Description = "GLogWare ==> PLC"
            },
            new LogPlcDirection {
                Name = LogPlcDirectionNames.PLC_TO_GLOGWARE.ToString(),
                TranslationKey = $"{typeof(LogPlcDirection).Name}.{LogPlcDirectionNames.PLC_TO_GLOGWARE.ToString()}",
                Description = "PLC ==> GLogWare"
            }
        };
    }
}
