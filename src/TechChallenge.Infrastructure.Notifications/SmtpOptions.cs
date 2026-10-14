namespace TechChallenge.Infrastructure.Notifications;

public class SmtpOptions
{
    public const string SectionName = "Smtp";

    public string Host { get; set; } = "localhost";
    public int Port { get; set; } = 1025;
    public bool UseSsl { get; set; } = true;
    public bool AllowInsecureConnection { get; set; }
    public string? Username { get; set; }
    public string? Password { get; set; }
    public string FromAddress { get; set; } = "nao-responda@oficina.local";
    public string FromName { get; set; } = "Oficina Tech Challenge";
}
