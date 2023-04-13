namespace UptimeKuma_API_CSharp_Sample.Models.KumaModels.Enums;

public enum KumaHeartbeatStatus
{
    /// <summary>
    /// 離線
    /// </summary>
    DOWN = 0,

    /// <summary>
    /// 上線
    /// </summary>
    UP = 1,

    /// <summary>
    /// 等待中
    /// </summary>
    PENDING = 2,

    /// <summary>
    /// 維護中
    /// </summary>
    MAINTENANCE = 3
}