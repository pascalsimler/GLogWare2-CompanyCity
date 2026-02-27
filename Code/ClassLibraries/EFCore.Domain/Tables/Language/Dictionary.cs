namespace Gudel.GLogWare.EFCore.Domain;

public class Dictionary : BaseTracking
{
    public string DicoRef { get; set; } = null!;
    public string Language{ get; set; } = null!;
    public string? Translation { get; set; }

    public Language LanguageRecord { get; set; } = null!;
}