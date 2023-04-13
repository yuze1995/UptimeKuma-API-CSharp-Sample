using Serilog;
using Serilog.Events;
using Serilog.Exceptions;
using Serilog.Formatting.Json;
using SocketIOClient;
using StackExchange.Redis;
using UptimeKuma_API_CSharp_Sample.Configurations;
using UptimeKuma_API_CSharp_Sample.Extensions.ServiceCollections;
using UptimeKuma_API_CSharp_Sample.Repositories;
using UptimeKuma_API_CSharp_Sample.Utilities.Json;

var builder = WebApplication.CreateBuilder(args);

var env = builder.Environment;
builder.Configuration.SetBasePath(env.ContentRootPath)
       .AddJsonFile("appsettings.json", true, true)
       .AddJsonFile($"appsettings.{env.EnvironmentName.Trim().ToLower()}.json", true, true)
       .AddEnvironmentVariables();

builder.Host.UseSerilog((context, serviceProvider, configuration) =>
{
    configuration
        .ReadFrom.Configuration(context.Configuration)
        .ReadFrom.Services(serviceProvider)
        .Enrich.FromLogContext()
        .Enrich.WithMachineName()
        .Enrich.WithExceptionStackTraceHash()
        .Enrich.WithExceptionData()
        .Enrich.WithExceptionDetails()
        .MinimumLevel.Override("Microsoft", LogEventLevel.Information)
        .MinimumLevel.Override("Microsoft.AspNetCore", LogEventLevel.Warning)
        .WriteTo.Console(new JsonFormatter(renderMessage: true));
    // .WriteTo.Console();
});

// Add services to the container.

builder.Services.AddControllers().AddJsonOptions(options =>
{
    options.JsonSerializerOptions.WriteIndented = JsonSerializerOptionsExtension.DefaultOptions.WriteIndented;
    options.JsonSerializerOptions.Encoder = JsonSerializerOptionsExtension.DefaultOptions.Encoder;
    foreach (var jsonConverter in JsonSerializerOptionsExtension.DefaultOptions.Converters)
    {
        options.JsonSerializerOptions.Converters.Add(jsonConverter);
    }
});
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.Configure<KumaConfig>(builder.Configuration.GetSection(nameof(KumaConfig)));

builder.Services.AddSingleton<IConnectionMultiplexer>(ConnectionMultiplexer.Connect(builder.Configuration.GetConnectionString("Redis")));
builder.Services.AddSingleton<IRedisRepository, RedisRepository>();

builder.Services.AddSocketIo();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

var socketIo = app.Services.GetRequiredService<SocketIO>();
await socketIo.ConnectAsync();

app.Run();