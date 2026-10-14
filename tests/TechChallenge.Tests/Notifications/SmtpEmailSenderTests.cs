using System.Net.Mail;
using FluentAssertions;
using Microsoft.Extensions.Options;
using TechChallenge.Infrastructure.Notifications;

namespace TechChallenge.Tests.Notifications;

public class SmtpEmailSenderTests
{
    [Fact]
    public async Task DeveBloquearSmtpSemTlsQuandoNaoHouverPermissaoExplicita()
    {
        var sender = CriarSender(new SmtpOptions
        {
            Host = "127.0.0.1",
            Port = 1,
            UseSsl = false,
            AllowInsecureConnection = false
        });

        var acao = () => sender.EnviarAsync(
            "cliente@teste.local",
            "Status da OS",
            "Conteúdo de teste");

        await acao.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*habilitação explícita*");
    }

    [Fact]
    public async Task DeveTentarConectarQuandoSmtpLocalSemTlsForPermitido()
    {
        var options = new SmtpOptions
        {
            Host = "127.0.0.1",
            Port = 1,
            UseSsl = false,
            AllowInsecureConnection = true,
            Username = "usuario",
            Password = "senha",
            FromAddress = "oficina@teste.local",
            FromName = "Oficina Teste"
        };
        var sender = CriarSender(options);

        var acao = () => sender.EnviarAsync(
            "cliente@teste.local",
            "Status da OS",
            "Conteúdo de teste");

        await acao.Should().ThrowAsync<SmtpException>();
        options.FromAddress.Should().Be("oficina@teste.local");
        options.FromName.Should().Be("Oficina Teste");
    }

    [Fact]
    public void DeveUsarTlsPorPadrao()
    {
        var options = new SmtpOptions();

        options.Host.Should().Be("localhost");
        options.Port.Should().Be(1025);
        options.UseSsl.Should().BeTrue();
        options.AllowInsecureConnection.Should().BeFalse();
    }

    private static SmtpEmailSender CriarSender(SmtpOptions options) =>
        new(Options.Create(options));
}
