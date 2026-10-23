using System.Diagnostics.Metrics;

namespace TechChallenge.Application.Abstractions.Observability;

/// <summary>
/// Métricas de negócio da oficina. Sem um MeterListener (OpenTelemetry) ativo,
/// as chamadas são no-op, portanto não alteram o comportamento em testes.
/// </summary>
public static class TechChallengeMetrics
{
    public const string MeterName = "TechChallenge.OrdensServico";

    private static readonly Meter Meter = new(MeterName);

    public static readonly Counter<long> OrdensCriadas = Meter.CreateCounter<long>(
        "techchallenge.os.criadas",
        unit: "{ordem}",
        description: "Ordens de serviço abertas.");

    public static readonly Counter<long> Transicoes = Meter.CreateCounter<long>(
        "techchallenge.os.transicoes",
        unit: "{transicao}",
        description: "Mudanças de status das ordens de serviço.");

    public static readonly Histogram<double> TempoNoStatus = Meter.CreateHistogram<double>(
        "techchallenge.os.tempo_status",
        unit: "s",
        description: "Tempo que a ordem de serviço permaneceu no status anterior à transição.",
        tags: null,
        advice: new InstrumentAdvice<double>
        {
            // De segundos (demonstração) a uma semana (operação real da oficina).
            HistogramBucketBoundaries = [5, 15, 30, 60, 300, 900, 1800, 3600, 7200, 14400, 28800, 86400, 172800, 604800]
        });

    public static readonly Counter<long> NotificacoesEnviadas = Meter.CreateCounter<long>(
        "techchallenge.notificacoes.enviadas",
        unit: "{notificacao}",
        description: "Notificações de status enviadas por e-mail.");

    public static readonly Counter<long> NotificacoesFalhas = Meter.CreateCounter<long>(
        "techchallenge.notificacoes.falhas",
        unit: "{notificacao}",
        description: "Falhas no envio de notificações de status.");

    public static readonly Counter<long> NotificacoesComFalha = Meter.CreateCounter<long>(
        "techchallenge.notificacoes.com_falha",
        unit: "{notificacao}",
        description: "Notificações que falharam na primeira tentativa (cada notificação conta uma vez).");

    public static readonly Counter<long> NotificacoesRecuperadas = Meter.CreateCounter<long>(
        "techchallenge.notificacoes.recuperadas",
        unit: "{notificacao}",
        description: "Notificações entregues após uma ou mais falhas.");

    public static readonly Counter<long> DecisoesExternas = Meter.CreateCounter<long>(
        "techchallenge.integracoes.decisoes",
        unit: "{decisao}",
        description: "Decisões de orçamento recebidas pela integração externa.");

    public static readonly Counter<long> FalhasProcessamento = Meter.CreateCounter<long>(
        "techchallenge.processamento.falhas",
        unit: "{falha}",
        description: "Exceções tratadas pela API, por área de negócio.");
}
