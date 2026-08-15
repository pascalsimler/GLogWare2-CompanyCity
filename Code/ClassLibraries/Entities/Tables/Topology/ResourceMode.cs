namespace Gudel.GLogWare.Entities;

public class ResourceMode : BaseTracking, ISeedData<ResourceMode>
{
    public string Identifier { get; set; } = null!;
    public string? DicoRef { get; set; } 
    public string? Description { get; set; }

    public ICollection<Resource> Resources { get; set; } = [];

    public static IEnumerable<ResourceMode> SeedData()
    {
        return [
            new() {
                Identifier = nameof(ResourceModeIdentifiers.UNDEFINED),
                DicoRef = $"{nameof(ResourceMode)}.{nameof(ResourceModeIdentifiers.UNDEFINED)}",
                Description = "Working mode is not defined",
            },
            new()
            {
                Identifier = nameof(ResourceModeIdentifiers.AUTOMATIC),
                DicoRef = $"{nameof(ResourceMode)}.{nameof(ResourceModeIdentifiers.AUTOMATIC)}",
                Description = "Only when in this mode, resource accepts orders from GLogWare",
            },
            new() {
                Identifier = nameof(ResourceModeIdentifiers.MANUAL),
                DicoRef = $"{nameof(ResourceMode)}.{nameof(ResourceModeIdentifiers.MANUAL)}",
                Description = "Orders can be generated manually on the HMI panel of the resource",
            },
            new() {
                Identifier = nameof(ResourceModeIdentifiers.STOPPED),
                DicoRef = $"{nameof(ResourceMode)}.{nameof(ResourceModeIdentifiers.STOPPED)}",
                Description = "No movement possible"
            },
        ];
    }
}
