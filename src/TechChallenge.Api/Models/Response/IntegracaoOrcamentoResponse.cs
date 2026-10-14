using TechChallenge.Domain.Enums;

namespace TechChallenge.Api.Models.Response;

public sealed record ReceberDecisaoOrcamentoExternaResponse(
    string EventoId,
    Guid OrdemServicoId,
    StatusOS Status,
    bool Processado,
    bool Duplicado);
