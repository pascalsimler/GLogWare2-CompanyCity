namespace Gudel.GLogWare.EFCore.Domain;

[SeedOrder(1)]
public class SkuType : ISeedData<SkuType>
{
    public string? Name { get; set; }
    public string? TranslationKey { get; set; }
    public string? Description { get; set; }

    public static IEnumerable<SkuType> SeedData()
    {
        return new List<SkuType>() {
            new SkuType {
                Name = nameof(SkuTypeIdentifiers.TIRE),
                TranslationKey = $"{nameof(SkuType)}.{nameof(SkuTypeIdentifiers.TIRE)}",
                Description = "Tire"
            },
            new SkuType {
                Name = nameof(SkuTypeIdentifiers.WHEEL),
                TranslationKey = $"{nameof(SkuType)}.{nameof(SkuTypeIdentifiers)}",
                Description = "Wheel"
            },
            new SkuType {
                Name = nameof(SkuTypeIdentifiers.CRATE),
                TranslationKey = $"{nameof(SkuType)}.{nameof(SkuTypeIdentifiers.CRATE)}",
                Description = "Crate"
            },
            new SkuType {
                Name = nameof(SkuTypeIdentifiers.PALLET),
                TranslationKey = $"{nameof(SkuType)}.{nameof(SkuTypeIdentifiers.PALLET)}",
                Description = "Pallet"
            },
        };
    }
}
