namespace Gudel.GLogWare.EFCore.Domain;

public class Protocol
{
    public long Id { get; set; }
    public DateTimeOffset Timestamp { get; set; } = DateTimeOffset.UtcNow;
    public string Level { get; set; } = default!;
    public string Message { get; set; } = default!;
    public string? Context { get; set; }
}