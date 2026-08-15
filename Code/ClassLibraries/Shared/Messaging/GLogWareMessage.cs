using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Gudel.GLogWare.Messages;

public class GLogWareMessage
{
    public DateTime Timestamp { get; set; } = DateTime.Now;
    public string Sender { get; set; } = string.Empty;
    [JsonRequired]
    public GLogWareMessageIdentifiers Identifier { get; set; }
    public object? Data { get; set; }

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true,
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
        Converters = { new JsonStringEnumConverter() }
    };

    public string Serialize()
    {
        return JsonSerializer.Serialize(this, JsonOptions);
    }

    public static string Serialize<T>(T o)
    {
        return JsonSerializer.Serialize(o, JsonOptions);
    }

    public static GLogWareMessage? DeSerialize(string jsonPayload)
    {
        return JsonSerializer.Deserialize<GLogWareMessage>(jsonPayload, JsonOptions);
    }

    public static T? DeSerialize<T>(string jsonPayload)
    {
        return JsonSerializer.Deserialize<T>(jsonPayload, JsonOptions);
    }

}
