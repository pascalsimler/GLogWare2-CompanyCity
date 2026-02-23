namespace Gudel.GLogWare.EFCore.Domain;

public class LogPlc
{
    public Int64 Id { get; set; }
    public string? Direction { get; set; }
    public string? Category { get; set; }
    public string? Sender { get; set; }
    public string? Receiver { get; set; }
    public string? Process { get; set; }
    public string? Identifier { get; set; }
    public string? Information { get; set; }
    public string? Data { get; set; }

    public LogPlcCategory PlcCategoryRecord { get; set; } = null!;
    public LogPlcDirection PlcDirectionRecord { get; set; } = null!;
}