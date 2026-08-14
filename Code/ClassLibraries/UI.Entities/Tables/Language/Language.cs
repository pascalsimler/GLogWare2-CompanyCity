namespace Gudel.GLogWare.UI.Entities;

public class Language : BaseTracking
{
    public string Code { get; set; } = null!;
    public string? Description { get; set; }

    public ICollection<Dictionary> Dictionaries { get; set; } = new List<Dictionary>();
}