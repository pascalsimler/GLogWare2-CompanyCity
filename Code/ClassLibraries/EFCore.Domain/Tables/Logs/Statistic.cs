namespace Gudel.GLogWare.EFCore.Domain;

public class Statistic
{
    public string Category { get; set; } = null!;
    public string Element { get; set; } = null!;
    public DateOnly Day { get; set; }
    public int Hour { get; set; }
}
