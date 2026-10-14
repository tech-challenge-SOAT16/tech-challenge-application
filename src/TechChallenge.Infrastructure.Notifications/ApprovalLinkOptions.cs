namespace TechChallenge.Infrastructure.Notifications;

public class ApprovalLinkOptions
{
    public const string SectionName = "ApprovalLinks";

    public string BaseUrl { get; set; } = "http://localhost:8080";
    public int ExpirationHours { get; set; } = 48;
}
