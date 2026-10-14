using TechChallenge.Api.Models.Request;
using TechChallenge.Api.Models.Response;
using TechChallenge.Application.Features.Estoque.AdicionarEstoque;
using TechChallenge.Application.Features.Estoque.BaixarEstoque;
using TechChallenge.Application.Features.Estoque.ListarEstoques;
using TechChallenge.Application.Features.Estoque.ObterEstoque;
using TechChallenge.Domain.Entities;

namespace TechChallenge.Api.Endpoints;

public static class EstoqueEndpoints
{
    public static IEndpointRouteBuilder MapEstoqueEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/v1/estoque")
            .WithName("Estoque");

        group.MapPost("/", AdicionarEstoqueAsync)
            .WithName("AdicionarEstoque")
            .RequireAuthorization("AdminOuVendedor")
            .WithSummary("Adiciona produto ao estoque")
            .WithDescription("Adiciona uma quantidade específica de um produto ao estoque")
            .Produces<Guid>(StatusCodes.Status201Created)
            .ProducesValidationProblem();

        group.MapGet("/", ListarEstoquesAsync)
            .WithName("ListarEstoques")
            .WithSummary("Lista todos os produtos em estoque")
            .WithDescription("Lista todos os produtos e suas quantidades em estoque")
            .Produces<List<EstoqueResponse>>();

        group.MapGet("/{produtoId:guid}", ObterEstoqueAsync)
            .WithName("ObterEstoque")
            .RequireAuthorization("AdminOuVendedor")
            .WithSummary("Obtém informações de um produto em estoque de acordo com o Id do produto informado")
            .WithDescription("Obtém informações sobre a quantidade disponível de um produto em estoque")
            .Produces<EstoqueResponse>()
            .Produces(StatusCodes.Status404NotFound);

        group.MapPut("/", BaixarEstoqueAsync)
            .WithName("BaixarEstoque")
            .RequireAuthorization("AdminOuVendedor")
            .WithSummary("Baixa produto do estoque")
            .WithDescription("Baixa uma quantidade específica de um produto do estoque")
            .Produces<EstoqueResponse>(StatusCodes.Status200OK)
            .ProducesValidationProblem()
            .Produces(StatusCodes.Status404NotFound);

        return app;
    }

    private static async Task<IResult> ListarEstoquesAsync(ListarEstoquesService service)
    {
        var estoques = await service.ListarEstoquesAsync(new ListarEstoquesQuery());
        return Results.Ok(estoques.Select(MapearEstoqueResponse).ToList());
    }

    private static async Task<IResult> ObterEstoqueAsync(Guid produtoId, ObterEstoqueService service)
    {
        var estoque = await service.ObterEstoqueAsync(new ObterEstoqueQuery { ProdutoId = produtoId });
        return Results.Ok(MapearEstoqueResponse(estoque));
    }

    private static async Task<IResult> AdicionarEstoqueAsync(AdicionarEstoqueRequest request, AdicionarEstoqueService service)
    {
        var command = new AdicionarEstoqueCommand
        {
            ProdutoId = request.ProdutoId,
            Quantidade = request.Quantidade
        };

        var estoque = await service.AdicionarEstoqueAsync(command);

        return Results.Created($"/api/v1/estoque/{estoque.ProdutoId}", MapearEstoqueResponse(estoque));
    }

    private static async Task<IResult> BaixarEstoqueAsync(BaixarEstoqueRequest request, BaixarEstoqueService service)
    {
        var command = new BaixarEstoqueCommand
        {
            ProdutoId = request.ProdutoId,
            Quantidade = request.Quantidade
        };

        var estoque = await service.BaixarEstoqueAsync(command);

        return Results.Ok(MapearEstoqueResponse(estoque));
    }

    private static EstoqueResponse MapearEstoqueResponse(Estoque estoque)
    {
        return new EstoqueResponse
        {
            ProdutoId = estoque.ProdutoId,
            Quantidade = estoque.Quantidade
        };
    }

}
