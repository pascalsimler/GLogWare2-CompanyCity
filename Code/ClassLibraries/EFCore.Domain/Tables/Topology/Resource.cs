namespace Gudel.GLogWare.EFCore.Domain;

[SeedOrder(2)]
public class Resource : BaseTracking, ISeedData<Resource>
{
    public string Name { get; set; } = null!;
    public string Mode { get; set; } = nameof(ResourceModeIdentifiers.UNDEFINED);
    public string? Description { get; set; }
    public bool IsOnline { get; set; } = false;
    public bool Parked { get; set; } = false;
    public bool Occupied { get; set; } = false;
    public bool ErrorFlag { get; set; } = true;
    public bool InfeedEnabled { get; set; } = true;
    public bool OutfeedEnabled { get; set; } = true;
    public bool RelocationEnabled { get; set; } = true;

    public ResourceMode ResourceModeRecord { get; set; } = null!;

    public static IEnumerable<Resource> SeedData()
    {
        return new List<Resource> {
            new Resource { 
                Name ="OP7100BR", 
                Description = "Bridge 1 of Gantry 1",
            },
            new Resource {
                Name ="OP7200BR",
                Description = "Bridge 2 of Gantry 1",
            },
            new Resource {
                Name ="OP7300BR",
                Description = "Bridge 3 of Gantry 1",
            },
            new Resource {
                Name ="OP7400BR",
                Description = "Bridge 4 of Gantry 1",
            },
            new Resource {
                Name ="OP7500BR",
                Description = "Bridge 5 of Gantry 1",
            },
             new Resource {
                Name ="OP8100BR",
                Description = "Bridge 1 of Gantry 2",
            },
            new Resource {
                Name ="OP8200BR",
                Description = "Bridge 2 of Gantry 2",
            },
            new Resource {
                Name ="OP8300BR",
                Description = "Bridge 3 of Gantry 2",
            },
            new Resource {
                Name ="OP8400BR",
                Description = "Bridge 4 of Gantry 2",
            },
            new Resource {
                Name ="OP8500BR",
                Description = "Bridge 5 of Gantry 2",
            },
        };
    }
}
