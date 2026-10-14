using System.Net;
using System.Text;
using TechChallenge.Application.Abstractions.Notifications;
using TechChallenge.Application.Features.OS.ReceberDecisaoOrcamentoExterna;
using TechChallenge.Domain.Enums;
using TechChallenge.Domain.Exceptions;

namespace TechChallenge.Api.Endpoints;

public static class DecisoesOrcamentoEmailEndpoints
{
    private const string MotivoRecusa = "Recusado pelo cliente através do link enviado por e-mail.";

    public static IEndpointRouteBuilder MapDecisoesOrcamentoEmailEndpoints(
        this IEndpointRouteBuilder app)
    {
        app.MapGet("/orcamentos/decisao", ExibirConfirmacao)
            .AllowAnonymous()
            .ExcludeFromDescription();

        app.MapPost("/orcamentos/decisao", ConfirmarDecisaoAsync)
            .AllowAnonymous()
            .ExcludeFromDescription();

        return app;
    }

    private static IResult ExibirConfirmacao(
        string? token,
        string? decisao,
        HttpContext httpContext,
        IDecisaoOrcamentoTokenService tokenService)
    {
        httpContext.Response.Headers.CacheControl = "no-store";

        var tokenValidado = tokenService.Validar(token ?? string.Empty);
        if (tokenValidado is null || !TryParseDecisao(decisao, out var decisaoValidada))
        {
            return Html(
                CriarPaginaMensagem(
                    "Link inválido ou expirado",
                    "Solicite um novo e-mail para decidir o orçamento."),
                StatusCodes.Status400BadRequest);
        }

        var acao = decisaoValidada == DecisaoOrcamento.Aprovado ? "aprovar" : "recusar";
        var acaoTitulo = decisaoValidada == DecisaoOrcamento.Aprovado ? "aprovação" : "recusa";
        var tokenHtml = WebUtility.HtmlEncode(token);
        var decisaoHtml = WebUtility.HtmlEncode(decisaoValidada.ToString());

        var conteudo = $$"""
            <h1>Confirmar {{acaoTitulo}} do orçamento</h1>
            <p>Você está prestes a <strong>{{acao}}</strong> o orçamento da ordem de serviço.</p>
            <p>A mudança só será realizada ao pressionar o botão abaixo.</p>
            <form method="post" action="/orcamentos/decisao">
              <input type="hidden" name="token" value="{{tokenHtml}}">
              <input type="hidden" name="decisao" value="{{decisaoHtml}}">
              <button type="submit">Confirmar {{acaoTitulo}}</button>
            </form>
            """;

        return Html(CriarPagina("Decisão do orçamento", conteudo));
    }

    private static async Task<IResult> ConfirmarDecisaoAsync(
        HttpRequest request,
        IDecisaoOrcamentoTokenService tokenService,
        ReceberDecisaoOrcamentoExternaService service)
    {
        request.HttpContext.Response.Headers.CacheControl = "no-store";
        var form = await request.ReadFormAsync(request.HttpContext.RequestAborted);
        var token = form["token"].ToString();
        var decisao = form["decisao"].ToString();
        var tokenValidado = tokenService.Validar(token);

        if (tokenValidado is null || !TryParseDecisao(decisao, out var decisaoValidada))
        {
            return Html(
                CriarPaginaMensagem(
                    "Link inválido ou expirado",
                    "Solicite um novo e-mail para decidir o orçamento."),
                StatusCodes.Status400BadRequest);
        }

        try
        {
            var resultado = await service.ReceberAsync(new ReceberDecisaoOrcamentoExternaCommand
            {
                EventoId = $"email-{tokenValidado.EventoId:N}",
                OrdemServicoId = tokenValidado.OrdemServicoId,
                Decisao = decisaoValidada,
                Motivo = decisaoValidada == DecisaoOrcamento.Recusado ? MotivoRecusa : null,
                OcorridoEm = tokenValidado.EmitidoEm
            });

            var titulo = ObterTituloResultado(decisaoValidada, resultado.Duplicado);
            var mensagem = resultado.Duplicado
                ? "Este link já havia sido utilizado com a mesma decisão."
                : "A oficina recebeu sua decisão com sucesso.";

            return Html(CriarPaginaMensagem(titulo, mensagem));
        }
        catch (DomainConflictException exception)
        {
            return Html(
                CriarPaginaMensagem("Decisão não realizada", exception.Message),
                StatusCodes.Status409Conflict);
        }
        catch (KeyNotFoundException exception)
        {
            return Html(
                CriarPaginaMensagem("Ordem de serviço não encontrada", exception.Message),
                StatusCodes.Status404NotFound);
        }
    }

    private static bool TryParseDecisao(string? value, out DecisaoOrcamento decisao)
    {
        return Enum.TryParse(value, ignoreCase: true, out decisao) &&
               Enum.IsDefined(decisao);
    }

    private static string ObterTituloResultado(
        DecisaoOrcamento decisao,
        bool duplicado)
    {
        if (duplicado)
            return "Decisão já registrada";

        return decisao == DecisaoOrcamento.Aprovado
            ? "Orçamento aprovado"
            : "Orçamento recusado";
    }

    private static string CriarPaginaMensagem(string titulo, string mensagem) =>
        CriarPagina(
            titulo,
            $"<h1>{WebUtility.HtmlEncode(titulo)}</h1><p>{WebUtility.HtmlEncode(mensagem)}</p>");

    private static string CriarPagina(string titulo, string conteudo) => $$"""
        <!doctype html>
        <html lang="pt-BR">
        <head>
          <meta charset="utf-8">
          <meta name="viewport" content="width=device-width,initial-scale=1">
          <title>{{WebUtility.HtmlEncode(titulo)}}</title>
          <style>
            body{font-family:Arial,sans-serif;background:#f3f4f6;color:#1f2937;margin:0;padding:24px}
            main{max-width:560px;margin:48px auto;background:#fff;padding:32px;border-radius:12px;box-shadow:0 8px 24px #00000014}
            h1{font-size:24px;margin-top:0}button{border:0;border-radius:6px;background:#1d4ed8;color:#fff;padding:12px 18px;font-size:16px;cursor:pointer}
          </style>
        </head>
        <body><main>{{conteudo}}</main></body>
        </html>
        """;

    private static IResult Html(string conteudo, int statusCode = StatusCodes.Status200OK) =>
        Results.Content(conteudo, "text/html", Encoding.UTF8, statusCode);
}
