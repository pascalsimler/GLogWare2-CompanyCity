using System.Text.Json.Serialization;

namespace Gudel.GLogWare.Shared;

public class PlcMessage
{
    public string? Sender { get; set; }
    public string? Receiver { get; set; }
    [JsonRequired]
    public PlcMessageIdentifiers Identifier { get; set; }
    public object? Data { get; set; }
}
