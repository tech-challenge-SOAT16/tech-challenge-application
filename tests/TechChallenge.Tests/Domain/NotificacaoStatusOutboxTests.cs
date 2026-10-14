using FluentAssertions;
using TechChallenge.Domain.Entities;
using TechChallenge.Domain.Enums;
using TechChallenge.Domain.Events;

namespace TechChallenge.Tests.Domain;

public class NotificacaoStatusOutboxTests
{
    [Fact]
    public void DeveRegistrarFalhaComBackoffELimitarMensagem()
    {
        var agora = DateTime.UtcNow;
        var notificacao = Criar(agora);

        notificacao.Reservar(agora, TimeSpan.FromSeconds(30));
        notificacao.RegistrarFalha(new string('x', 1200), agora);

        notificacao.Tentativas.Should().Be(1);
        notificacao.ProximaTentativaEm.Should().Be(agora.AddSeconds(5));
        notificacao.BloqueadaAte.Should().BeNull();
        notificacao.UltimoErro.Should().HaveLength(1000);
    }

    [Fact]
    public void DeveMarcarComoEnviadaELiberarReserva()
    {
        var agora = DateTime.UtcNow;
        var notificacao = Criar(agora);
        notificacao.Reservar(agora, TimeSpan.FromSeconds(30));

        notificacao.MarcarComoEnviada(agora.AddSeconds(1));

        notificacao.EnviadaEm.Should().Be(agora.AddSeconds(1));
        notificacao.BloqueadaAte.Should().BeNull();
        notificacao.UltimoErro.Should().BeNull();
    }

    private static NotificacaoStatusOutbox Criar(DateTime criadaEm) => new(
        Guid.NewGuid(),
        new StatusOrdemServicoAlteradoEvent(
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            "OS-TESTE",
            StatusOS.EmDiagnostico,
            StatusOS.AguardandoAprovacao,
            criadaEm));
}
