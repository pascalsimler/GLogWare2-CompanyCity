using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Gudel.GLogWare.Shared;

public class GLogWareMessage
{
    public DateTime Timestamp { get; set; } = DateTime.Now;
    public string Sender { get; set; } = string.Empty;
    [JsonRequired]
    public GLogWareMessageIdentifiers Identifier { get; set; }
    public object? Data { get; set; }

    public string Serialize()
    {
        var options = new JsonSerializerOptions
        {
            WriteIndented = true,
            Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
            Converters = { new JsonStringEnumConverter() }
        };

        return JsonSerializer.Serialize(this, options);
    }

    public static GLogWareMessage? DeSerialize(string jsonPayload)
    {
        var options = new JsonSerializerOptions
        {
            WriteIndented = true,
            Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
            Converters = { new JsonStringEnumConverter() }
        };

        return JsonSerializer.Deserialize<GLogWareMessage>(jsonPayload, options);
    }

    public static T? DeSerialize<T>(string jsonPayload)
    {
        var options = new JsonSerializerOptions
        {
            WriteIndented = true,
            Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
            Converters = { new JsonStringEnumConverter() }
        };

        return JsonSerializer.Deserialize<T>(jsonPayload, options);
    }

}
