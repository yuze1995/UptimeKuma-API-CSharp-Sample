using System.Text.Encodings.Web;
using System.Text.Json;
using UptimeKuma_API_CSharp_Sample.Utilities.Json.Converters;

namespace UptimeKuma_API_CSharp_Sample.Utilities.Json;

public static class JsonSerializerOptionsExtension
{
    public static JsonSerializerOptions DefaultOptions =>
        new(JsonSerializerDefaults.Web)
        {
            WriteIndented = false,
            Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
            Converters =
            {
                new BoolConverter(),
                new DateTimeConverter()
            }
        };
}