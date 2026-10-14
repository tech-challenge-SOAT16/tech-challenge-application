using TechChallenge.Domain.Enums;

namespace TechChallenge.Domain.Entities;

public class DecisaoOrcamentoExterna
{
    public Guid Id { get; private set; }
    public Guid OrdemServicoId { get; private set; }
    public string EventoId { get; private set; } = string.Empty;
    public DecisaoOrcamento Decisao { get; private set; }
    public string? Motivo { get; private set; }
    public DateTime OcorridoEm { get; private set; }
    public DateTime RecebidoEm { get; private set; }

    private DecisaoOrcamentoExterna()
    {
    }

    public DecisaoOrcamentoExterna(
        Guid id,
        Guid ordemServicoId,
        string eventoId,
        DecisaoOrcamento decisao,
        string? motivo,
        DateTime ocorridoEm,
        DateTime recebidoEm)
    {
        Id = id;
        OrdemServicoId = ordemServicoId;
        EventoId = eventoId;
        Decisao = decisao;
        Motivo = motivo;
        OcorridoEm = ocorridoEm;
        RecebidoEm = recebidoEm;
    }

    public bool CorrespondeA(DecisaoOrcamento decisao, string? motivo, DateTime ocorridoEm)
    {
        return Decisao == decisao &&
               string.Equals(Motivo, NormalizarMotivo(motivo), StringComparison.Ordinal) &&
               OcorridoEm == ocorridoEm;
    }

    public static string? NormalizarMotivo(string? motivo) =>
        string.IsNullOrWhiteSpace(motivo) ? null : motivo.Trim();
}
