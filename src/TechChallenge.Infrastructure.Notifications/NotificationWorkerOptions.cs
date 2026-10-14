namespace TechChallenge.Infrastructure.Notifications;

public class NotificationWorkerOptions
{
    public const string SectionName = "Notifications";

    public bool Enabled { get; set; }
    public int IntervalSeconds { get; set; } = 2;
    public int BatchSize { get; set; } = 10;
    public int LockSeconds { get; set; } = 60;
}
