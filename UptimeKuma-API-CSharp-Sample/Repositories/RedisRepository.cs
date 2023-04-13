using StackExchange.Redis;
using System.Text;
using System.Text.Json;
using UptimeKuma_API_CSharp_Sample.Consts;
using UptimeKuma_API_CSharp_Sample.Models.KumaModels;
using UptimeKuma_API_CSharp_Sample.Utilities.Json;
using ILogger = Serilog.ILogger;

namespace UptimeKuma_API_CSharp_Sample.Repositories;

public interface IRedisRepository
{
    Task<IEnumerable<KumaMonitor>> GetMonitors(CancellationToken token = default);

    Task<KumaMonitor> GetMonitorById(long monitorId, CancellationToken token = default);

    Task<bool> BackupMonitors(IEnumerable<object> monitors, CancellationToken token = default);

    Task<bool> BackupNotifications(IEnumerable<object> notifications, CancellationToken token = default);

    Task<bool> BackupVersion(string version);

    Task<bool> SetStatus(string monitorName, string status);
}

public class RedisRepository : IRedisRepository
{
    private readonly ILogger _logger;
    private readonly IDatabase _db;

    public RedisRepository(ILogger logger, IConnectionMultiplexer connectionMultiplexer)
    {
        _logger = logger;
        _db = connectionMultiplexer.GetDatabase();
    }

    public async Task<IEnumerable<KumaMonitor>> GetMonitors(CancellationToken token = default)
    {
        var json = await _db.StringGetAsync(BackupCacheKey.MonitorList);

        if (string.IsNullOrEmpty(json) is false)
        {
            var ms = new MemoryStream(Encoding.UTF8.GetBytes(json.ToString()));

            return await JsonSerializer.DeserializeAsync<IEnumerable<KumaMonitor>>(ms, JsonSerializerOptionsExtension.DefaultOptions, token);
        }

        return Enumerable.Empty<KumaMonitor>();
    }

    public async Task<KumaMonitor> GetMonitorById(long monitorId, CancellationToken token = default)
    {
        var monitors = await GetMonitors(token);

        return monitors.Any() ? monitors.FirstOrDefault(p => p.Id == monitorId) : default;
    }

    public async Task<bool> BackupMonitors(IEnumerable<object> monitors, CancellationToken token = default)
    {
        if (monitors is null || monitors.Any() is false)
        {
            var value = await _db.StringGetAsync(BackupCacheKey.NotificationList);
            if (value.IsNullOrEmpty is false)
            {
                return false;
            }
        }

        var json = await ObjToJson(monitors, token);

        return await _db.StringSetAsync(BackupCacheKey.MonitorList, json);
    }

    public async Task<bool> BackupNotifications(IEnumerable<object> notifications, CancellationToken token = default)
    {
        if (notifications is null || notifications.Any() is false)
        {
            var value = await _db.StringGetAsync(BackupCacheKey.NotificationList);
            if (value.IsNullOrEmpty is false)
            {
                return false;
            }
        }

        var json = await ObjToJson(notifications, token);

        return await _db.StringSetAsync(BackupCacheKey.NotificationList, json);
    }

    public async Task<bool> BackupVersion(string version) => await _db.StringSetAsync(BackupCacheKey.Version, version);

    public async Task<bool> SetStatus(string monitorName, string status)
    {
        if (string.IsNullOrWhiteSpace(monitorName))
        {
            throw new ArgumentNullException(nameof(monitorName));
        }

        return await _db.StringSetAsync($"ServiceHealth:{monitorName.Trim().ToUpper()}:Status", status);
    }

    private static async Task<string> ObjToJson(object obj, CancellationToken token)
    {
        string json;

        using (var ms = new MemoryStream())
        {
            await JsonSerializer.SerializeAsync(ms, obj, JsonSerializerOptionsExtension.DefaultOptions, token);
            ms.Seek(0, SeekOrigin.Begin);
            json = Encoding.UTF8.GetString(ms.ToArray());
        }

        return json;
    }
}