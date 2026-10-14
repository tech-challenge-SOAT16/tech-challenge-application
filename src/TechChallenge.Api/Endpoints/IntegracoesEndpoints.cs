using TechChallenge.Api.Filters;
using TechChallenge.Api.Models.Request;
using TechChallenge.Api.Models.Response;
using TechChallenge.Application.Features.OS.ReceberDecisaoOrcamentoExterna;

namespace TechChallenge.Api.Endpoints;

public static class IntegracoesEndpoints
{
    public static IEndpointRouteBuilder MapIntegracoesEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/v1/integracoes")
            .WithTags("Integrações externas")
            .AllowAnonymous();

        group.MapPost("/orcamentos/respostas", ReceberDecisaoOrcamentoExternaAsync)
            .WithName("ReceberDecisaoOrcamentoExterna")
            .WithSummary("Recebe a aprovação ou recusa externa de um orçamento")
            .WithDescription("Processa uma decisão idempotente identificada por evento externo.")
            .AddEndpointFilter<IntegrationApiKeyFilter>()
            .Produces<ReceberDecisaoOrcamentoExternaResponse>()
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status409Conflict)
            .ProducesValidationProblem();

        return app;
    }

    private static async Task<IResult> ReceberDecisaoOrcamentoExternaAsync(
        ReceberDecisaoOrcamentoExternaRequest request,
        ReceberDecisaoOrcamentoExternaService service)
    {
        var resultado = await service.ReceberAsync(new ReceberDecisaoOrcamentoExternaCommand
        {
            EventoId = request.EventoId ?? string.Empty,
            OrdemServicoId = request.OrdemServicoId,
            Decisao = request.Decisao,
            Motivo = request.Motivo,
            OcorridoEm = request.OcorridoEm
        });

        return Results.Ok(new ReceberDecisaoOrcamentoExternaResponse(
            resultado.EventoId,
            resultado.OrdemServicoId,
            resultado.Status,
            resultado.Processado,
            resultado.Duplicado));
    }
}
