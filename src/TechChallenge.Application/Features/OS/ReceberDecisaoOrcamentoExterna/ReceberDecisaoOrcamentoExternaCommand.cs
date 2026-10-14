using TechChallenge.Domain.Enums;

namespace TechChallenge.Application.Features.OS.ReceberDecisaoOrcamentoExterna;

public class ReceberDecisaoOrcamentoExternaCommand
{
    public string EventoId { get; set; } = string.Empty;
    public Guid OrdemServicoId { get; set; }
    public DecisaoOrcamento Decisao { get; set; }
    public string? Motivo { get; set; }
    public DateTimeOffset OcorridoEm { get; set; }
}
