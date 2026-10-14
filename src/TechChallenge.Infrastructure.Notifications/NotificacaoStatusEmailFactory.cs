using System.Globalization;
using System.Net;
using Microsoft.Extensions.Options;
using TechChallenge.Application.Abstractions.Notifications;
using TechChallenge.Domain.Entities;
using TechChallenge.Domain.Enums;
using TechChallenge.Domain.Helpers;

namespace TechChallenge.Infrastructure.Notifications;

public sealed class NotificacaoStatusEmailFactory
{
    private readonly IDecisaoOrcamentoTokenService _tokenService;
    private readonly ApprovalLinkOptions _options;

    public NotificacaoStatusEmailFactory(
        IDecisaoOrcamentoTokenService tokenService,
        IOptions<ApprovalLinkOptions> options)
    {
        _tokenService = tokenService;
        _options = options.Value;
    }

    public NotificacaoStatusEmail Criar(
        NotificacaoStatusOutbox notificacao,
        string nomeCliente,
        OrcamentoEmailResumo? orcamento = null)
    {
        var cliente = WebUtility.HtmlEncode(nomeCliente);
        var codigo = WebUtility.HtmlEncode(notificacao.CodigoAcompanhamento);
        var statusAnterior = WebUtility.HtmlEncode(
            SystemHelper.GetStatusDescription(notificacao.StatusAnterior));
        var statusAtualDescricao = SystemHelper.GetStatusDescription(notificacao.StatusAtual);
        var statusAtual = WebUtility.HtmlEncode(statusAtualDescricao);

        var acoes = notificacao.StatusAtual == StatusOS.AguardandoAprovacao
            ? CriarAcoesOrcamento(
                notificacao,
                orcamento ?? throw new InvalidOperationException(
                    "O resumo do orçamento é obrigatório no e-mail de aprovação."))
            : string.Empty;

        var conteudo = $$"""
            <!doctype html>
            <html lang="pt-BR">
            <body style="font-family:Arial,sans-serif;color:#1f2937;line-height:1.5">
              <div style="max-width:600px;margin:0 auto;padding:24px">
                <h2 style="margin-top:0">Atualização da ordem de serviço</h2>
                <p>Olá, {{cliente}}.</p>
                <p>
                  A ordem de serviço <strong>{{codigo}}</strong> mudou de
                  <strong>{{statusAnterior}}</strong> para <strong>{{statusAtual}}</strong>.
                </p>
                {{acoes}}
                <p style="color:#6b7280;font-size:14px">
                  Consulte o acompanhamento da OS para mais detalhes.
                </p>
              </div>
            </body>
            </html>
            """;

        var assunto = notificacao.StatusAtual == StatusOS.AguardandoAprovacao
            ? "Ação necessária: aprove ou recuse o orçamento"
            : $"OS Atualizada: {statusAtualDescricao}";

        return new NotificacaoStatusEmail(assunto, conteudo);
    }

    private string CriarAcoesOrcamento(
        NotificacaoStatusOutbox notificacao,
        OrcamentoEmailResumo orcamento)
    {
        var criadaEmUtc = notificacao.CriadaEm.Kind == DateTimeKind.Unspecified
            ? DateTime.SpecifyKind(notificacao.CriadaEm, DateTimeKind.Utc)
            : notificacao.CriadaEm.ToUniversalTime();
        var validade = TimeSpan.FromHours(_options.ExpirationHours);
        var token = _tokenService.Gerar(
            notificacao.EventoId,
            notificacao.OrdemServicoId,
            new DateTimeOffset(criadaEmUtc),
            validade);
        var tokenUrl = Uri.EscapeDataString(token);
        var baseUrl = _options.BaseUrl.TrimEnd('/');
        var aprovarUrl = WebUtility.HtmlEncode(
            $"{baseUrl}/orcamentos/decisao?token={tokenUrl}&decisao=Aprovado");
        var recusarUrl = WebUtility.HtmlEncode(
            $"{baseUrl}/orcamentos/decisao?token={tokenUrl}&decisao=Recusado");
        var resumo = CriarResumoOrcamento(orcamento);

        return $$"""
            <div style="margin:28px 0;padding:20px;background:#f3f4f6;border-radius:8px">
              <p style="margin-top:0"><strong>O orçamento está pronto para sua decisão.</strong></p>
              {{resumo}}
              <p>Escolha uma opção. A decisão será confirmada na próxima tela.</p>
              <p style="margin-bottom:0">
                <a href="{{aprovarUrl}}" style="display:inline-block;padding:12px 18px;margin-right:8px;background:#15803d;color:#fff;text-decoration:none;border-radius:6px">Aprovar orçamento</a>
                <a href="{{recusarUrl}}" style="display:inline-block;padding:12px 18px;background:#b91c1c;color:#fff;text-decoration:none;border-radius:6px">Recusar orçamento</a>
              </p>
              <p style="margin-bottom:0;color:#6b7280;font-size:12px">
                Links válidos por {{_options.ExpirationHours}} horas.
              </p>
            </div>
            """;
    }

    private static string CriarResumoOrcamento(OrcamentoEmailResumo orcamento)
    {
        var linhas = orcamento.Servicos
            .Select(item => CriarLinhaItem("Serviço", item))
            .Concat(orcamento.Produtos.Select(item => CriarLinhaItem("Produto", item)))
            .ToList();

        if (orcamento.Acrescimo > 0)
            linhas.Add(CriarLinhaValor("Acréscimo", orcamento.Acrescimo));

        if (orcamento.Desconto > 0)
            linhas.Add(CriarLinhaValor("Desconto", -orcamento.Desconto));

        linhas.Add($$"""
            <tr style="font-weight:bold;border-top:2px solid #d1d5db">
              <td style="padding:10px 4px">Total</td>
              <td style="padding:10px 4px;text-align:right">{{FormatarMoeda(orcamento.Total)}}</td>
            </tr>
            """);

        return $$"""
            <h3 style="margin-bottom:8px">Resumo do orçamento</h3>
            <table style="width:100%;border-collapse:collapse;margin-bottom:18px">
              {{string.Join(Environment.NewLine, linhas)}}
            </table>
            """;
    }

    private static string CriarLinhaItem(string tipo, OrcamentoEmailItem item) => $$"""
        <tr style="border-top:1px solid #d1d5db">
          <td style="padding:8px 4px">{{tipo}}: {{WebUtility.HtmlEncode(item.Descricao)}} ({{item.Quantidade}}x)</td>
          <td style="padding:8px 4px;text-align:right">{{FormatarMoeda(item.ValorTotal)}}</td>
        </tr>
        """;

    private static string CriarLinhaValor(string descricao, double valor) => $$"""
        <tr style="border-top:1px solid #d1d5db">
          <td style="padding:8px 4px">{{WebUtility.HtmlEncode(descricao)}}</td>
          <td style="padding:8px 4px;text-align:right">{{FormatarMoeda(valor)}}</td>
        </tr>
        """;

    private static string FormatarMoeda(double valor) =>
        valor.ToString("C", CultureInfo.GetCultureInfo("pt-BR"));
}

public sealed record NotificacaoStatusEmail(string Assunto, string ConteudoHtml);

public sealed record OrcamentoEmailResumo(
    IReadOnlyList<OrcamentoEmailItem> Servicos,
    IReadOnlyList<OrcamentoEmailItem> Produtos,
    double Acrescimo,
    double Desconto,
    double Total);

public sealed record OrcamentoEmailItem(
    string Descricao,
    int Quantidade,
    double ValorTotal);
