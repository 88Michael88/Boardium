namespace Boardium.Models.ServiceModels;

public class EmailSettings
{
    public string Host { get; set; } = null!;
    public int Port { get; set; }
    public bool EnableSsl { get; set; }
    public string UserName { get; set; } = null!;
    public string Password { get; set; } = null!;
    public string From { get; set; } = null!;
}