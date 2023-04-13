using System.Text.Json.Serialization;

namespace UptimeKuma_API_CSharp_Sample.Models.KumaModels;

public class KumaMonitor
{
    [JsonPropertyName("id")]
    public long Id { get; set; }

    [JsonPropertyName("name")]
    public string Name { get; set; }

    [JsonPropertyName("active")]
    public bool Active { get; set; }
}