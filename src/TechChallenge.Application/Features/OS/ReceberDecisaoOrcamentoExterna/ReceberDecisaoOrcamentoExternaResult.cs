using TechChallenge.Domain.Enums;

namespace TechChallenge.Application.Features.OS.ReceberDecisaoOrcamentoExterna;

public sealed record ReceberDecisaoOrcamentoExternaResult(
    string EventoId,
    Guid OrdemServicoId,
    StatusOS Status,
    bool Processado,
    bool Duplicado);
