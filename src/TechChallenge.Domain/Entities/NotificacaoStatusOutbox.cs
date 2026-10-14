using TechChallenge.Domain.Enums;
using TechChallenge.Domain.Events;

namespace TechChallenge.Domain.Entities;

public class NotificacaoStatusOutbox
{
    public Guid Id { get; private set; }
    public Guid EventoId { get; private set; }
    public Guid OrdemServicoId { get; private set; }
    public Guid ClienteId { get; private set; }
    public Cliente Cliente { get; private set; } = null!;
    public string CodigoAcompanhamento { get; private set; } = string.Empty;
    public StatusOS StatusAnterior { get; private set; }
    public StatusOS StatusAtual { get; private set; }
    public DateTime CriadaEm { get; private set; }
    public DateTime? EnviadaEm { get; private set; }
    public DateTime ProximaTentativaEm { get; private set; }
    public DateTime? BloqueadaAte { get; private set; }
    public int Tentativas { get; private set; }
    public string? UltimoErro { get; private set; }
    public int Versao { get; private set; }

    private NotificacaoStatusOutbox()
    {
    }

    public NotificacaoStatusOutbox(
        Guid id,
        StatusOrdemServicoAlteradoEvent evento)
    {
        Id = id;
        EventoId = evento.EventoId;
        OrdemServicoId = evento.OrdemServicoId;
        ClienteId = evento.ClienteId;
        CodigoAcompanhamento = evento.CodigoAcompanhamento;
        StatusAnterior = evento.StatusAnterior;
        StatusAtual = evento.StatusAtual;
        CriadaEm = evento.OcorridoEm;
        ProximaTentativaEm = evento.OcorridoEm;
    }

    public void Reservar(DateTime agora, TimeSpan duracaoBloqueio)
    {
        BloqueadaAte = agora.Add(duracaoBloqueio);
        Versao++;
    }

    public void MarcarComoEnviada(DateTime enviadaEm)
    {
        EnviadaEm = enviadaEm;
        BloqueadaAte = null;
        UltimoErro = null;
        Versao++;
    }

    public void RegistrarFalha(string erro, DateTime agora)
    {
        Tentativas++;
        UltimoErro = erro.Length <= 1000 ? erro : erro[..1000];
        BloqueadaAte = null;
        ProximaTentativaEm = agora.Add(CalcularEspera(Tentativas));
        Versao++;
    }

    private static TimeSpan CalcularEspera(int tentativas)
    {
        var segundos = Math.Min(300, 5 * Math.Pow(2, Math.Min(tentativas - 1, 6)));
        return TimeSpan.FromSeconds(segundos);
    }
}
