namespace Gudel.GLogWare.EFCore.Domain;

[SeedOrder(1)]
public class Area : BaseTracking, ISeedData<Area>
{
    public string Name { get; set; } = null!;
    public string? Comments { get; set; }

    public ICollection<Place> Places { get; set; } = new List<Place>();

    public static IEnumerable<Area> SeedData()
    {
        return new List<Area>() {
            new Area { Name = AreaConstants.GantryStorage, Comments = "Gantry area cell" },
            new Area { Name = AreaConstants.GantryBridge, Comments = "Gantry bridge" },
            new Area { Name = AreaConstants.Conveyor, Comments = "Conveyor belt" }
        };
    }
}
