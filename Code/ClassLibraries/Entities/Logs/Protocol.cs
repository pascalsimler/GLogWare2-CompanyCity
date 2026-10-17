namespace Gudel.GLogWare.Entities;

public class Protocol
{
    public long Id { get; set; }
    public DateTime? Timestamp { get; set; } = DateTime.Now;
    public string Message { get; set; } = default!;
}