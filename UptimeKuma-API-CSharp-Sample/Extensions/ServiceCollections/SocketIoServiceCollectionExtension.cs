using Microsoft.Extensions.Options;
using Serilog.Context;
using SocketIOClient;
using UptimeKuma_API_CSharp_Sample.Configurations;
using UptimeKuma_API_CSharp_Sample.Consts.KumaEvents;
using UptimeKuma_API_CSharp_Sample.Models.KumaModels;
using UptimeKuma_API_CSharp_Sample.Models.KumaModels.Enums;
using UptimeKuma_API_CSharp_Sample.Models.KumaModels.Response;
using UptimeKuma_API_CSharp_Sample.Repositories;
using UptimeKuma_API_CSharp_Sample.Utilities.Json;
using ILogger = Serilog.ILogger;

namespace UptimeKuma_API_CSharp_Sample.Extensions.ServiceCollections;

public static class SocketIoServiceCollectionExtension
{
    public static void AddSocketIo(this IServiceCollection services)
    {
        services.AddSingleton<SocketIO>(serviceProvider =>
        {
            var logger = serviceProvider.GetRequiredService<ILogger>();
            var kumaConfig = serviceProvider.GetRequiredService<IOptions<KumaConfig>>();
            var redisRepository = serviceProvider.GetRequiredService<IRedisRepository>();

            var socketIo = new SocketIO(kumaConfig.Value.ServerUrl)
            {
                JsonSerializer = new SocketIOClient.JsonSerializer.SystemTextJsonSerializer(JsonSerializerOptionsExtension.DefaultOptions)
            };

            void Invoke(SocketIOResponse response, string eventName, Func<CancellationToken, Task> func)
            {
                LogContext.PushProperty("Event", eventName);
                LogContext.PushProperty("EventProcessId", Guid.NewGuid());
                LogContext.PushProperty("SocketIOSessionId", response.SocketIO.Id);

                var respStr = response.ToString();

                try
                {
                    logger.Information("Event Received [{Event}], {Response}", eventName, respStr);
                    func?.Invoke(new CancellationTokenSource().Token).ConfigureAwait(false).GetAwaiter().GetResult();
                }
                catch (Exception e)
                {
                    logger.Error(e, "Got Error when execute event [{Event}], Response: {Response}", eventName, respStr);
                }
                finally
                {
                    logger.Information("Event executed Done [{Event}]", eventName);
                }
            }

            socketIo.On(KumaReceivedEvent.Heartbeat,
                        resp =>
                            Invoke(resp,
                                   KumaReceivedEvent.Heartbeat,
                                   async (token) =>
                                   {
                                       var heartbeat = resp.GetValue<KumaHeartbeat>();

                                       if (heartbeat?.Important is true)
                                       {
                                           var monitor = await redisRepository.GetMonitorById(heartbeat.MonitorId, token);

                                           if (monitor != null)
                                           {
                                               await redisRepository.SetStatus(monitor?.Name, heartbeat?.Status.ToString());
                                           }
                                       }
                                   }));

            socketIo.On(KumaReceivedEvent.ImportantHeartbeatList,
                        resp =>
                            Invoke(resp,
                                   KumaReceivedEvent.ImportantHeartbeatList,
                                   async (token) =>
                                   {
                                       var monitorId = resp.GetValue<long>(0);
                                       var importantHeartbeat = resp.GetValue<IEnumerable<KumaHeartbeat>>(1);

                                       if (importantHeartbeat is not null && importantHeartbeat.Any())
                                       {
                                           var monitor = await redisRepository.GetMonitorById(monitorId, token);

                                           if (monitor?.Active is true)
                                           {
                                               var latestHeartbeat = importantHeartbeat.Where(p => p.Important).MaxBy(p => p.Time);
                                               await redisRepository.SetStatus(monitor?.Name, latestHeartbeat?.Status.ToString());
                                           }
                                       }
                                   }));

            // 1. 後續要匯入還原時，需要從這邊拿資料並組成 json 寫進去
            socketIo.On(KumaReceivedEvent.MonitorList,
                        resp =>
                            Invoke(resp,
                                   KumaReceivedEvent.MonitorList,
                                   async (token) =>
                                   {
                                       var monitors = resp.GetValue<Dictionary<string, object>>();
                                       await redisRepository.BackupMonitors(monitors.Values, token);
                                   }));

            // 1. 後續要匯入還原時，需要從這邊拿資料並組成 json 寫進去
            socketIo.On(KumaReceivedEvent.NotificationList,
                        resp =>
                            Invoke(resp,
                                   KumaReceivedEvent.NotificationList,
                                   async (token) =>
                                   {
                                       var notifications = resp.GetValue<IEnumerable<object>>();
                                       await redisRepository.BackupNotifications(notifications, token);
                                   }));

            // 1. 後續要匯入還原時，需要從這邊拿資料並組成 json 寫進去
            socketIo.On(KumaReceivedEvent.Info,
                        resp =>
                            Invoke(resp,
                                   KumaReceivedEvent.Info,
                                   async (token) =>
                                   {
                                       var info = resp.GetValue<KumaServerInfo>();
                                       await redisRepository.BackupVersion(info.Version);
                                   }));

            socketIo.OnConnected += async (sender, e) =>
            {
                logger.Information($"{nameof(SocketIO.OnConnected)}");
                await socketIo.Login(logger, kumaConfig.Value.AdminAccount);
            };

            socketIo.OnReconnected += async (sender, e) =>
            {
                logger.Information($"{nameof(SocketIO.OnReconnected)}, Total Reconnect Try Count: {e}");
                await socketIo.Login(logger, kumaConfig.Value.AdminAccount);
            };

            socketIo.OnDisconnected += async (sender, e) =>
            {
                logger.Error($"{nameof(SocketIO.OnDisconnected)}, {e}");
            };

            socketIo.OnReconnectAttempt += async (sender, e) =>
            {
                logger.Information($"{nameof(SocketIO.OnReconnectAttempt)}, Reconnect Counter: {e}");
            };

            // 尚未達到 Reconnect 次數限制的 Retry Error
            socketIo.OnReconnectError += async (sender, e) =>
            {
                logger.Error(e, $"{nameof(SocketIO.OnReconnectFailed)}");
            };

            // 達到 Reconnect 次數限制的 Retry Error
            socketIo.OnReconnectFailed += async (sender, e) =>
            {
                logger.Error($"{nameof(SocketIO.OnReconnectFailed)}");
            };

            socketIo.OnError += async (sender, e) =>
            {
                logger.Error($"{nameof(SocketIO.OnError)}, {e}");

                var monitors = await redisRepository.GetMonitors();

                var tasks = monitors.Select(monitor => redisRepository.SetStatus(monitor?.Name, KumaHeartbeatStatus.UP.ToString()));
                await Task.WhenAll(tasks);
            };

            return socketIo;
        });
    }

    private static async Task Login(this SocketIO client, ILogger logger, AdminAccount account)
    {
        var dto = new
        {
            username = account.Username,
            password = account.Password
        };

        await client.EmitAsync(KumaEmitEvent.Login, response =>
        {
            var result = response.GetValue<LoginEmitResp>();

            if (result.Ok)
            {
                logger.Information("Login Success, {Username}", account.Username);
            }
            else
            {
                logger.Error("Login Failed, {Username}, {ErrorMessage}", account.Username, result.Message);
            }
        }, dto);
    }
}