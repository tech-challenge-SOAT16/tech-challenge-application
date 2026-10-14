using TechChallenge.Api.Models.Request;
using TechChallenge.Api.Models.Response;
using TechChallenge.Application.Features.Inventario.AtualizarItemInventario;
using TechChallenge.Application.Features.Inventario.CriarItemInventario;
using TechChallenge.Application.Features.Inventario.ExcluirItemInventario;
using TechChallenge.Application.Features.Inventario.ListarInventario;
using TechChallenge.Application.Features.Inventario.ObterItemInventario;
using TechChallenge.Domain.Entities;

namespace TechChallenge.Api.Endpoints;

public static class InventarioEndpoints
{
    public static IEndpointRouteBuilder MapInventarioEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/v1/produtos")
            .WithName("Inventário");

        group.MapPost("/", CriarItemInventarioAsync)
            .WithName("CriarItemInventario")
            .RequireAuthorization("AdminOuVendedor")
            .WithSummary("Cria um novo item de inventário")
            .WithDescription("Adiciona um novo item de inventário ao banco de dados")
            .Produces<Guid>(StatusCodes.Status201Created)
            .ProducesValidationProblem();

        group.MapGet("/{id}", ObterItemInventarioAsync)
            .WithName("ObterItemInventario")
            .WithSummary("Obtém um item de inventário existente")
            .WithDescription("Obtém as informações de um item de inventário existente do banco de dados")
            .Produces<ProdutoResponse>()
            .Produces(StatusCodes.Status404NotFound);

        group.MapGet("/", ListarInventarioAsync)
            .WithName("ListarInventario")
            .WithSummary("Lista todos os itens de inventário")
            .WithDescription("Lista todos os itens de inventário do banco de dados")
            .Produces<List<ProdutoResponse>>();

        group.MapPut("/{id}", AtualizarItemInventarioAsync)
            .WithName("AtualizarItemInventario")
            .RequireAuthorization("AdminOuVendedor")
            .WithSummary("Atualiza um item de inventário existente")
            .WithDescription("Atualiza as informações de um item de inventário existente no banco de dados")
            .ProducesValidationProblem();

        group.MapDelete("/{id}", ExcluirItemInventarioAsync)
            .WithName("ExcluirItemInventario")
            .RequireAuthorization("AdminOuVendedor")
            .WithSummary("Exclui um item de inventário existente")
            .WithDescription("Exclui um item de inventário existente do banco de dados")
            .ProducesValidationProblem();

        return app;
    }

    private static async Task<IResult> CriarItemInventarioAsync(CriarProdutoRequest request, CriarItemInventarioService service)
    {
        var command = new CriarItemInventarioCommand
        {
            Descricao = request.Descricao,
            Valor = request.Valor,
            Quantidade = request.Quantidade,
            IdCategoria = request.IdCategoria
        };

        var id = await service.CriarItemInventarioAsync(command);
        return Results.Created($"/api/v1/produtos/{id}", id);
    }

    private static async Task<IResult> ObterItemInventarioAsync(Guid id, ObterItemInventarioService service)
    {
        var produto = await service.ObterItemInventario(new ObterItemInventarioQuery { Id = id });
        return Results.Ok(MapearProdutoResponse(produto));
    }

    private static async Task<IResult> ListarInventarioAsync(ListarInventarioService service)
    {
        var produtos = await service.ListarInventario(new ListarInventarioQuery());
        return Results.Ok(produtos.Select(MapearProdutoResponse).ToList());
    }

    private static async Task<IResult> AtualizarItemInventarioAsync(Guid id, AtualizarProdutoRequest request, AtualizarItemInventarioService service)
    {
        var command = new AtualizarItemInventarioCommand
        {
            Id = id,
            Descricao = request.Descricao,
            Valor = request.Valor,
            Quantidade = request.Quantidade
        };

        await service.AtualizarItemInventarioAsync(command);
        return Results.Ok();
    }

    private static async Task<IResult> ExcluirItemInventarioAsync(Guid id, ExcluirItemInventarioService service)
    {
        await service.ExcluirItemInventarioAsync(new ExcluirItemInventarioCommand { Id = id });
        return Results.NoContent();
    }

    private static ProdutoResponse MapearProdutoResponse(Produto produto)
    {
        return new ProdutoResponse
        {
            Id = produto.Id,
            Descricao = produto.Descricao,
            Valor = produto.Valor,
            Quantidade = produto.Estoque?.Quantidade ?? 0,
            CategoriaId = produto.CategoriaId
        };
    }
}
