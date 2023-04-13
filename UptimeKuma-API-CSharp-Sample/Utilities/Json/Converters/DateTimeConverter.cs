using System.Text.Json;
using System.Text.Json.Serialization;

namespace UptimeKuma_API_CSharp_Sample.Utilities.Json.Converters;

public class DateTimeConverter : JsonConverter<DateTime>
{
    public override DateTime Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        switch (reader.TokenType)
        {
            case JsonTokenType.String:
            {
                _ = DateTime.TryParse(reader.GetString(), out var dateTime);
                return dateTime;
            }
            default: throw new JsonException();
        }
    }

    public override void Write(Utf8JsonWriter writer, DateTime value, JsonSerializerOptions options) => writer.WriteStringValue(value.ToString("yyyy-MM-dd HH:mm:ss"));
}