using System.Text.Json.Serialization;

namespace UptimeKuma_API_CSharp_Sample.Models.KumaModels;

public class KumaServerInfo
{
    [JsonPropertyName("version")]
    public string Version { get; set; }

    [JsonPropertyName("latestVersion")]
    public string LatestVersion { get; set; }

    [JsonPropertyName("primaryBaseURL")]
    public string PrimaryBaseURL { get; set; }

    [JsonPropertyName("serverTimezone")]
    public string ServerTimezone { get; set; }

    [JsonPropertyName("serverTimezoneOffset")]
    public string ServerTimezoneOffset { get; set; }
}