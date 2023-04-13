namespace UptimeKuma_API_CSharp_Sample.Configurations;

public class KumaConfig
{
    public string ServerUrl { get; set; }

    public AdminAccount AdminAccount { get; set; }
}

public class AdminAccount
{
    public string Username { get; set; }

    public string Password { get; set; }
}