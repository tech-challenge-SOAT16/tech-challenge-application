using TechChallenge.Domain.Enums;

namespace TechChallenge.Api.Models.Request;

public class ReceberDecisaoOrcamentoExternaRequest
{
    public string? EventoId { get; set; }
    public Guid OrdemServicoId { get; set; }
    public DecisaoOrcamento Decisao { get; set; }
    public string? Motivo { get; set; }
    public DateTimeOffset OcorridoEm { get; set; }
}
