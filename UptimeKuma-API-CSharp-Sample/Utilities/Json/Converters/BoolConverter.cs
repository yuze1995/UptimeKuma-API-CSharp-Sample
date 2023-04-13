﻿using System.Text.Json;
using System.Text.Json.Serialization;

namespace UptimeKuma_API_CSharp_Sample.Utilities.Json.Converters;

public class BoolConverter : JsonConverter<bool>
{
    public override bool Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) =>
        reader.TokenType switch
        {
            JsonTokenType.True => true,
            JsonTokenType.False => false,
            JsonTokenType.String => bool.TryParse(reader.GetString(), out var b) ? b : throw new JsonException(),
            JsonTokenType.Number => reader.TryGetInt64(out var l) ? Convert.ToBoolean(l) : reader.TryGetDouble(out var d) && Convert.ToBoolean(d),
            _ => throw new JsonException(),
        };

    public override void Write(Utf8JsonWriter writer, bool value, JsonSerializerOptions options) => writer.WriteBooleanValue(value);
}
