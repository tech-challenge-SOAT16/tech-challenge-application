using TechChallenge.Domain.Enums;

namespace TechChallenge.Domain.Events;

public sealed record StatusOrdemServicoAlteradoEvent(
    Guid EventoId,
    Guid OrdemServicoId,
    Guid ClienteId,
    string CodigoAcompanhamento,
    StatusOS StatusAnterior,
    StatusOS StatusAtual,
    DateTime OcorridoEm);
