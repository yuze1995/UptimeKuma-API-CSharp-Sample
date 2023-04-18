using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using SocketIOClient;
using StackExchange.Redis;
using UptimeKuma_API_CSharp_Sample.Configurations;
using UptimeKuma_API_CSharp_Sample.Consts;
using ILogger = Serilog.ILogger;

namespace UptimeKuma_API_CSharp_Sample.Controllers;

[ApiController]
[Route("[controller]")]
public class OperationController : ControllerBase
{
    private readonly ILogger _logger;
    private readonly KumaConfig _kumaConfig;
    private readonly SocketIO _socketIo;
    private readonly IDatabase _database;

    public OperationController(ILogger logger, IOptions<KumaConfig> kumaSettings, SocketIO socketIo, IConnectionMultiplexer connectionMultiplexer)
    {
        _kumaConfig = kumaSettings.Value;
        _logger = logger;
        _socketIo = socketIo;
        _database = connectionMultiplexer.GetDatabase();
    }

    [HttpGet("connect-kuma")]
    public async Task<IActionResult> ConnectKuma()
    {
        await _socketIo.ConnectAsync();

        var dto = new
        {
            username = _kumaConfig.AdminAccount.Username,
            password = _kumaConfig.AdminAccount.Password
        };

        await _socketIo.EmitAsync("login",
                                  response =>
                                  {
                                      _logger.Information("{Response}", response);
                                  },
                                  dto);

        return Ok("Connected!");
    }

    [HttpGet("add-monitor")]
    public async Task<IActionResult> AddMonitors()
    {
        var monitors = new Dictionary<string, string>()
        {
        };

        var tasks = monitors.Select(monitor => _socketIo.EmitAsync("add", () =>
        {
            _logger.Information("Add Monitor: {MonitorName}", monitor.Key);

            return GenerateAddMonitorDto(monitor.Key, monitor.Value);
        }));

        await Task.WhenAll(tasks);

        return Ok("Connected!");
    }

    private static object GenerateAddMonitorDto(string name, string url)
    {
        var dto = new
        {
            type = "http",
            name = name,
            url = url,
            method = "GET",
            interval = 20,
            retryInterval = 20,
            resendInterval = 0,
            maxretries = 2,
            notificationIDList = new Dictionary<string, bool>()
            {
                { "1", true }
            },
            ignoreTls = false,
            upsideDown = false,
            packetSize = 56,
            expiryNotification = false,
            maxredirects = 1,
            accepted_statuscodes = new List<string>() { "200-299" },
            dns_resolve_type = "A",
            dns_resolve_server = "1.1.1.1",
            docker_container = "",
            docker_host = default(string),
            proxyId = default(string),
            mqttUsername = "",
            mqttPassword = "",
            mqttTopic = "",
            mqttSuccessMessage = "",
            authMethod = default(string),
            httpBodyEncoding = "json"
        };

        return dto;
    }

    [HttpGet("monitorList")]
    public async Task<IActionResult> GetMonitorListFromCache()
    {
        var monitors = await _database.StringGetAsync(BackupCacheKey.MonitorList);

        return Ok(monitors.ToString());
    }

    [HttpGet("notificationList")]
    public async Task<IActionResult> GetNotificationListFromCache()
    {
        var notifications = await _database.StringGetAsync(BackupCacheKey.NotificationList);

        return Ok(notifications.ToString());
    }

    [HttpGet("version")]
    public async Task<IActionResult> GetVersionFromCache()
    {
        var info = await _database.StringGetAsync(BackupCacheKey.Version);

        return Ok(info.ToString());
    }

    [HttpGet("heartbeat")]
    public async Task<IActionResult> GetHeartbeatFromCache(string monitorName)
    {
        var status = await _database.StringGetAsync($"ServiceHealth:{monitorName.Trim().ToUpper()}:Status");

        return Ok(status.ToString());
    }
}