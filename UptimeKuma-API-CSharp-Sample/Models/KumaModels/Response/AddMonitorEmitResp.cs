using System.Text.Json.Serialization;

namespace UptimeKuma_API_CSharp_Sample.Models.KumaModels.Response;

public class AddMonitorEmitResp
{
    [JsonPropertyName("ok")]
    public bool Ok { get; set; }

    [JsonPropertyName("msg")]
    public string Message { get; set; }

    [JsonPropertyName("monitorID")]
    public long MonitorId { get; set; }
}