using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using TechChallenge.Application.Abstractions.Notifications;

namespace TechChallenge.Infrastructure.Notifications;

public static class DependencyInjection
{
    public static IServiceCollection AddNotificationInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddOptions<SmtpOptions>()
            .Bind(configuration.GetSection(SmtpOptions.SectionName));

        services.AddOptions<NotificationWorkerOptions>()
            .Bind(configuration.GetSection(NotificationWorkerOptions.SectionName));

        services.AddOptions<ApprovalLinkOptions>()
            .Bind(configuration.GetSection(ApprovalLinkOptions.SectionName))
            .Validate(
                options => Uri.TryCreate(options.BaseUrl, UriKind.Absolute, out var uri) &&
                           (uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps),
                "ApprovalLinks:BaseUrl deve ser uma URL HTTP ou HTTPS absoluta.")
            .Validate(options => options.ExpirationHours is >= 1 and <= 168,
                "ApprovalLinks:ExpirationHours deve estar entre 1 e 168 horas.")
            .ValidateOnStart();

        services.AddSingleton<IEmailSender, SmtpEmailSender>();
        services.AddSingleton<NotificacaoStatusEmailFactory>();

        if (configuration.GetValue<bool>($"{NotificationWorkerOptions.SectionName}:Enabled"))
            services.AddHostedService<NotificacaoStatusOutboxWorker>();

        return services;
    }
}
