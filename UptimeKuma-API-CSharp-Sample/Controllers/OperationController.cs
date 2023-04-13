using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using SocketIOClient;
using StackExchange.Redis;
using UptimeKuma_API_CSharp_Sample.Configurations;
using UptimeKuma_API_CSharp_Sample.Consts;

namespace UptimeKuma_API_CSharp_Sample.Controllers;

[ApiController]
[Route("[controller]")]
public class OperationController : ControllerBase
{
    private readonly KumaConfig _kumaConfig;
    private readonly SocketIO _socketIo;
    private readonly IDatabase _database;

    public OperationController(IOptions<KumaConfig> kumaSettings, SocketIO socketIo, IConnectionMultiplexer connectionMultiplexer)
    {
        _kumaConfig = kumaSettings.Value;
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
                                      Console.WriteLine(response);
                                  },
                                  dto);

        return Ok("Connected!");
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