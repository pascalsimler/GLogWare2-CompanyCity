namespace Gudel.GLogWare.Entities;

[SeedOrder(1)]
public class Area : BaseTracking, ISeedData<Area>
{
    public string Name { get; set; } = null!;
    public string? Comments { get; set; }

    public ICollection<Place> Places { get; set; } = [];

    public static IEnumerable<Area> SeedData()
    {
        return [
            new() { Name = AreaIdentifiers.GANTRY.ToString(), Comments = "Gantry area cell" },
            new() { Name = AreaIdentifiers.BRIDGE.ToString(), Comments = "Gantry bridge" },
            new() { Name = AreaIdentifiers.CONV.ToString(), Comments = "Conveyor belt" }
        ];
    }
}
