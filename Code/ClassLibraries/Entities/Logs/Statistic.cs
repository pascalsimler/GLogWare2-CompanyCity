namespace Gudel.GLogWare.Entities;

public class Statistic
{
    public string Category { get; set; } = null!;
    public string Element { get; set; } = null!;
    public DateOnly Day { get; set; } = DateOnly.FromDateTime(DateTime.Now);
    public int Hour { get; set; }

    public int Counter01 { get; set; } = 0;
    public int Counter02 { get; set; } = 0;
    public int Counter03 { get; set; } = 0;
    public int Counter04 { get; set; } = 0;
    public int Counter05 { get; set; } = 0;
    public int Counter06 { get; set; } = 0;
    public int Counter07 { get; set; } = 0;
    public int Counter08 { get; set; } = 0;
    public int Counter09 { get; set; } = 0;
    public int Counter10 { get; set; } = 0;
    public int Counter11 { get; set; } = 0;
    public int Counter12 { get; set; } = 0;
    public int Counter13 { get; set; } = 0;
    public int Counter14 { get; set; } = 0;
    public int Counter15 { get; set; } = 0;
    public int Counter16 { get; set; } = 0;
    public int Counter17 { get; set; } = 0;
    public int Counter18 { get; set; } = 0;
    public int Counter19 { get; set; } = 0;
    public int Counter20 { get; set; } = 0;
    public StatisticCategory StatisticCategoryRecord { get; set; } = null!;

    public void SetCounterValue(int index, int value)
    {
        string propertyName = $"Counter{index:00}";

        var prop = this.GetType().GetProperty(propertyName);
        if (prop == null) return;
        prop.SetValue(this, value);
    }

}
