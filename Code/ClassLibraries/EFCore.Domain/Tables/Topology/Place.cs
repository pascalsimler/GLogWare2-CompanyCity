namespace Gudel.GLogWare.EFCore.Domain;

[SeedOrder(2)]
public class Place : BaseTracking, ISeedData<Place>
{
    public string Name { get; set; } = null!;
    public string? Area { get; set; } = null;
    public string? PlaceType { get; set; } = null;

    public string G { get; set; } = null!;
    public string XCell { get; set; } = null!;
    public string YCell { get; set; } = null!;
    public int XPos { get; set; }
    public int YPos { get; set; }

    public int Zone { get; set; }
    public int Distance { get; set; }

    public Area AreaRecord { get; set; } = null!;
    public PlaceType PlaceTypeRecord { get; set; } = null!;

    public ICollection<Sku> Skus { get; set; } = new List<Sku>();

    public static IEnumerable<Place> SeedData()
    {
        List<Place> places = new List<Place>();
        Place p;

        for (int g=1; g<=2; g++)
        {
            for (int x = 1; x <= 70; x++)
            {
                for (int y = 1; y <= 17; y++)
                {
                    p = new Place();
                    p.Name = $"{AreaConstants.GantryStorage}-{g:0}-{x:00}.{y:00}";
                    p.Area = AreaConstants.GantryStorage;
                    p.PlaceType = null;
                    p.G = g.ToString();
                    p.XCell = x.ToString();
                    p.YCell = y.ToString();
                    places.Add(p);
                }
            }
        }

        return places.OrderBy(p => p.Name);
    }
}