namespace Gudel.GLogWare.EFCore.Domain;

public class Protocol
{
    public long Id { get; set; }
    public DateTimeOffset Timestamp { get; set; } = DateTimeOffset.Now;
    public string Message { get; set; } = default!;
}