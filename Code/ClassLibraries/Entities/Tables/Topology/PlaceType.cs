namespace Gudel.GLogWare.Entities;

[SeedOrder(1)]
public class PlaceType : BaseTracking, ISeedData<PlaceType>
{
    public string Name { get; set; } = null!;
    public string? TranslationKey { get; set; }
    public string? Description { get; set; }

    public ICollection<Place> Places { get; set; } = [];


    public static IEnumerable<PlaceType> SeedData()
    {
        return [
            new() {
                Name = nameof(PlaceTypeIdentifiers.GANTRY_PICK),
                TranslationKey = $"{nameof(PlaceType)}.{nameof(PlaceTypeIdentifiers.GANTRY_PICK)}",
                Description = "Conveyor pick position"
            },
            new() {
                Name = nameof(PlaceTypeIdentifiers.GANTRY_DROP),
                TranslationKey = $"{nameof(PlaceType)}.{nameof(PlaceTypeIdentifiers.GANTRY_DROP)}",
                Description = "Conveyor drop position"
            }
        ];
    }
}
