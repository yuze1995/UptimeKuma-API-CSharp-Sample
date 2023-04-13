using System.Text.Json.Serialization;

namespace UptimeKuma_API_CSharp_Sample.Models.KumaModels;

public class KumaNotification
{
    [JsonPropertyName("id")]
    public long Id { get; set; }

    [JsonPropertyName("name")]
    public string Name { get; set; }

    [JsonPropertyName("config")]
    public string Config { get; set; }

    [JsonPropertyName("active")]
    public bool Active { get; set; }

    [JsonPropertyName("userId")]
    public long UserId { get; set; }

    [JsonPropertyName("isDefault")]
    public bool IsDefault { get; set; }
}