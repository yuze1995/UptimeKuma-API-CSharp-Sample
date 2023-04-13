using System.Text.Json.Serialization;

namespace UptimeKuma_API_CSharp_Sample.Models.KumaModels.Response;

public class LoginEmitResp
{
    [JsonPropertyName("ok")]
    public bool Ok { get; set; }

    [JsonPropertyName("token")]
    public string Token { get; set; }

    [JsonPropertyName("msg")]
    public string Message { get; set; }

    [JsonPropertyName("tokenRequired")]
    public string TokenRequired { get; set; } 
}