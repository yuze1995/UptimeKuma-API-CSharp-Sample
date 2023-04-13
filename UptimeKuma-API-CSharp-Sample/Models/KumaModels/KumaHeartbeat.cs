using System.Text.Json.Serialization;
using UptimeKuma_API_CSharp_Sample.Models.KumaModels.Enums;

namespace UptimeKuma_API_CSharp_Sample.Models.KumaModels;

public class KumaHeartbeat
{
    [JsonPropertyName("monitorID")]
    public long MonitorId { get; set; }

    /// <summary>
    /// 當次的 Heartbeat 結果
    /// </summary>
    [JsonPropertyName("status")]
    public KumaHeartbeatStatus Status { get; set; }

    [JsonPropertyName("time")]
    public DateTime Time { get; set; }

    [JsonPropertyName("msg")]
    public string Message { get; set; }

    [JsonPropertyName("ping")]
    public long? Ping { get; set; }

    /// <summary>
    /// 紀錄該筆 Heartbeat 是否為狀態變更的 Heartbeat
    /// </summary>
    [JsonPropertyName("important")]
    public bool Important { get; set; }

    /// <summary>
    /// 跟上一次 Heartbeat 的時間間隔 (seconds)
    /// </summary>
    [JsonPropertyName("duration")]
    public long Duration { get; set; } 
}