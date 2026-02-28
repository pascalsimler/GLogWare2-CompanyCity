using System.Text.Json.Serialization;

namespace Gudel.GLogWare.Shared;

public class PlcMessage
{
    [JsonRequired]
    public PlcMessageIdentifiers Identifier { get; set; }
    public object? Data { get; set; }
}
