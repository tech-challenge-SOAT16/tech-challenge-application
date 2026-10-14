namespace TechChallenge.Application.Abstractions.Notifications;

public interface IEmailSender
{
    Task EnviarAsync(
        string destinatario,
        string assunto,
        string conteudo,
        bool isHtml = false,
        CancellationToken cancellationToken = default);
}
