namespace Gudel.GLogWare.EFCore.Domain;

[SeedOrder(1)]
public class PlaceType : BaseTracking, ISeedData<PlaceType>
{
    public string Name { get; set; } = null!;

    public ICollection<Place> Places { get; set; } = new List<Place>();


    public static IEnumerable<PlaceType> SeedData()
    {
        return new List<PlaceType>();
    }
}
