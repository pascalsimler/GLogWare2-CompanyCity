namespace Gudel.GLogWare.EFCore.Domain;

[SeedOrder(2)]
public class Resource : BaseTracking, ISeedData<Resource>
{
    public string Name { get; set; } = null!;
    public string Mode { get; set; } = null!;
    public bool? Parked { get; set; }
    public bool? Occupied { get; set; }
    public bool? ErrorFlag { get; set; }
    public bool? InfeedEnabled { get; set; }
    public bool? OutfeedEnabled { get; set; }
    public bool? RelocationEnabled { get; set; }

    public ResourceMode ResourceModeRecord { get; set; } = null!;

    public static IEnumerable<Resource> SeedData()
    {
        return new List<Resource> {
            new Resource { 
                Name ="OP7100BR", 
                Mode = nameof(ResourceModeIdentifiers.UNDEFINED),
                Parked = false,
                Occupied = false,
                ErrorFlag = true,
                InfeedEnabled = true,
                OutfeedEnabled = true,
                RelocationEnabled = true,
            },
            new Resource {
                Name ="OP7200BR",
                Mode = nameof(ResourceModeIdentifiers.UNDEFINED),
                Parked = false,
                Occupied = false,
                ErrorFlag = true,
                InfeedEnabled = true,
                OutfeedEnabled = true,
                RelocationEnabled = true,
            },
            new Resource {
                Name ="OP7300BR",
                Mode = nameof(ResourceModeIdentifiers.UNDEFINED),
                Parked = false,
                Occupied = false,
                ErrorFlag = true,
                InfeedEnabled = true,
                OutfeedEnabled = true,
                RelocationEnabled = true,
            },
            new Resource {
                Name ="OP7400BR",
                Mode = nameof(ResourceModeIdentifiers.UNDEFINED),
                Parked = false,
                Occupied = false,
                ErrorFlag = true,
                InfeedEnabled = true,
                OutfeedEnabled = true,
                RelocationEnabled = true,
            },
            new Resource {
                Name ="OP7500BR",
                Mode = nameof(ResourceModeIdentifiers.UNDEFINED),
                Parked = false,
                Occupied = false,
                ErrorFlag = true,
                InfeedEnabled = true,
                OutfeedEnabled = true,
                RelocationEnabled = true,
            },
        };
    }
}
