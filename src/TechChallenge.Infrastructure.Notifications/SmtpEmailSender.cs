using System.Net;
using System.Net.Mail;
using Microsoft.Extensions.Options;
using TechChallenge.Application.Abstractions.Notifications;

namespace TechChallenge.Infrastructure.Notifications;

public class SmtpEmailSender : IEmailSender
{
    private readonly SmtpOptions _options;

    public SmtpEmailSender(IOptions<SmtpOptions> options)
    {
        _options = options.Value;
    }

    public async Task EnviarAsync(
        string destinatario,
        string assunto,
        string conteudo,
        bool isHtml = false,
        CancellationToken cancellationToken = default)
    {
        if (!_options.UseSsl && !_options.AllowInsecureConnection)
        {
            throw new InvalidOperationException(
                "SMTP sem TLS exige habilitação explícita para o ambiente local.");
        }

        using var mensagem = new MailMessage
        {
            From = new MailAddress(_options.FromAddress, _options.FromName),
            Subject = assunto,
            Body = conteudo,
            IsBodyHtml = isHtml
        };
        mensagem.To.Add(destinatario);

        using var smtpClient = new SmtpClient(_options.Host, _options.Port)
        {
            DeliveryMethod = SmtpDeliveryMethod.Network,
            UseDefaultCredentials = false
        };

        if (_options.UseSsl)
            smtpClient.EnableSsl = true;

        if (!string.IsNullOrWhiteSpace(_options.Username))
            smtpClient.Credentials = new NetworkCredential(_options.Username, _options.Password);

        await smtpClient.SendMailAsync(mensagem, cancellationToken);
    }
}
