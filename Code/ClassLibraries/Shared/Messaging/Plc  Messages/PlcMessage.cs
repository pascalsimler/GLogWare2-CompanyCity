using System.Text.Json.Serialization;

namespace Gudel.GLogWare.Messages;

public class PlcMessage
{
    public string Sender { get; set; } = string.Empty;
    public string Receiver { get; set; } = string.Empty;
    [JsonRequired]
    public PlcMessageIdentifiers Identifier { get; set; }
    public object? Data { get; set; }
}
