namespace TechChallenge.Application.Abstractions.Notifications;

public interface IDecisaoOrcamentoTokenService
{
    string Gerar(
        Guid eventoId,
        Guid ordemServicoId,
        DateTimeOffset emitidoEm,
        TimeSpan validade);

    DecisaoOrcamentoToken? Validar(string token);
}

public sealed record DecisaoOrcamentoToken(
    Guid EventoId,
    Guid OrdemServicoId,
    DateTimeOffset EmitidoEm,
    DateTimeOffset ExpiraEm);
