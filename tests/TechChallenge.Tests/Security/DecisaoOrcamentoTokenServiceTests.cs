using FluentAssertions;
using Microsoft.Extensions.Options;
using TechChallenge.Infrastructure.Auth;

namespace TechChallenge.Tests.Security;

public class DecisaoOrcamentoTokenServiceTests
{
    private static readonly DateTimeOffset Agora = new(2026, 9, 1, 12, 0, 0, TimeSpan.Zero);

    [Fact]
    public void DeveGerarEValidarTokenAssinado()
    {
        var timeProvider = new FixedTimeProvider(Agora);
        var service = CriarService(timeProvider);
        var eventoId = Guid.NewGuid();
        var ordemServicoId = Guid.NewGuid();

        var token = service.Gerar(eventoId, ordemServicoId, Agora, TimeSpan.FromHours(48));
        var resultado = service.Validar(token);

        resultado.Should().NotBeNull();
        resultado!.EventoId.Should().Be(eventoId);
        resultado.OrdemServicoId.Should().Be(ordemServicoId);
        resultado.EmitidoEm.Should().Be(Agora);
        resultado.ExpiraEm.Should().Be(Agora.AddHours(48));
    }

    [Fact]
    public void DeveRejeitarTokenAlterado()
    {
        var service = CriarService(new FixedTimeProvider(Agora));
        var token = service.Gerar(
            Guid.NewGuid(),
            Guid.NewGuid(),
            Agora,
            TimeSpan.FromHours(48));
        var ultimoCaractere = token[^1] == 'A' ? 'B' : 'A';
        var tokenAlterado = token[..^1] + ultimoCaractere;

        service.Validar(tokenAlterado).Should().BeNull();
    }

    [Fact]
    public void DeveRejeitarTokenExpirado()
    {
        var timeProvider = new FixedTimeProvider(Agora);
        var service = CriarService(timeProvider);
        var token = service.Gerar(
            Guid.NewGuid(),
            Guid.NewGuid(),
            Agora,
            TimeSpan.FromHours(48));
        timeProvider.Avancar(TimeSpan.FromHours(49));

        service.Validar(token).Should().BeNull();
    }

    private static DecisaoOrcamentoTokenService CriarService(TimeProvider timeProvider) =>
        new(
            Options.Create(new ApprovalLinkTokenOptions
            {
                SigningKey = "chave-de-teste-com-mais-de-32-caracteres-123456"
            }),
            timeProvider);

    private sealed class FixedTimeProvider(DateTimeOffset agora) : TimeProvider
    {
        private DateTimeOffset _agora = agora;

        public override DateTimeOffset GetUtcNow() => _agora;

        public void Avancar(TimeSpan intervalo) => _agora = _agora.Add(intervalo);
    }
}
