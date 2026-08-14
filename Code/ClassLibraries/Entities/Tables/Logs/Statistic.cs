namespace Gudel.GLogWare.Entities;

public class Statistic
{
    public string Category { get; set; } = null!;
    public string Element { get; set; } = null!;
    public DateOnly Day { get; set; } = DateOnly.FromDateTime(DateTime.Now);
    public int Hour { get; set; }

    public int Counter01 { get; set; }
    public int Counter02 { get; set; }
    public int Counter03 { get; set; }
    public int Counter04 { get; set; }
    public int Counter05 { get; set; }
    public int Counter06 { get; set; }
    public int Counter07 { get; set; }
    public int Counter08 { get; set; }
    public int Counter09 { get; set; }

    public StatisticCategory StatisticCategoryRecord { get; set; } = null!;
}
