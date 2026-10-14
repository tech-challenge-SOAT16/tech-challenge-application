using TechChallenge.Api.Models.Request;
using TechChallenge.Api.Models.Response;
using TechChallenge.Application.Features.CategoriaProdutos.AtualizarCategoriaProduto;
using TechChallenge.Application.Features.CategoriaProdutos.CriarCategoriaProduto;
using TechChallenge.Application.Features.CategoriaProdutos.ExcluirCategoriaProduto;
using TechChallenge.Application.Features.CategoriaProdutos.ListarCategoriasProdutos;
using TechChallenge.Application.Features.CategoriaProdutos.ObterCategoriaProduto;
using TechChallenge.Domain.Entities;

namespace TechChallenge.Api.Endpoints;

public static class CategoriaProdutoEndpoints
{
    public static IEndpointRouteBuilder MapCategoriaProdutoEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/v1/categoriaproduto")
            .WithName("CategoriaProduto");

        group.MapPost("/", CriarCategoriaProdutoAsync)
            .WithName("CriarCategoriaProduto")
            .RequireAuthorization("AdminOuVendedor")
            .WithSummary("Cria uma nova categoria de produto")
            .WithDescription("Adiciona uma nova categoria de produto ao banco de dados")
            .Produces<Guid>(StatusCodes.Status201Created)
            .ProducesValidationProblem();

        group.MapGet("/{id}", ObterCategoriaProdutoAsync)
            .WithName("ObterCategoriaProduto")
            .WithSummary("Obtém uma categoria de produto existente")
            .WithDescription("Obtém as informações de uma categoria de produto existente do banco de dados")
            .Produces<CategoriaProdutoResponse>()
            .Produces(StatusCodes.Status404NotFound);

        group.MapGet("/", ListarCategoriasProdutoAsync)
            .WithName("ListarCategoriasProduto")
            .WithSummary("Lista todas as categorias de produto")
            .WithDescription("Lista todas as categorias de produto do banco de dados")
            .Produces<List<CategoriaProdutoResponse>>();

        group.MapPut("/{id}", AtualizarCategoriaProdutoAsync)
            .WithName("AtualizarCategoriaProduto")
            .RequireAuthorization("AdminOuVendedor")
            .WithSummary("Atualiza uma categoria de produto existente")
            .WithDescription("Atualiza as informações de uma categoria de produto existente no banco de dados")
            .ProducesValidationProblem();

        group.MapDelete("/{id}", ExcluirCategoriaProdutoAsync)
            .WithName("ExcluirCategoriaProduto")
            .RequireAuthorization("AdminOuVendedor")
            .WithSummary("Exclui uma categoria de produto existente")
            .WithDescription("Exclui uma categoria de produto existente do banco de dados")
            .ProducesValidationProblem();

        return app;
    }

    private static async Task<IResult> CriarCategoriaProdutoAsync(CriarCategoriaProdutoRequest request, CriarCategoriaProdutoService service)
    {
        var command = new CriarCategoriaProdutoCommand
        {
            Descricao = request.Descricao ?? string.Empty
        };

        var id = await service.CriarCategoriaProduto(command);
        return Results.Created($"/api/v1/categoriaproduto/{id}", id);
    }

    private static async Task<IResult> AtualizarCategoriaProdutoAsync(Guid id, AtualizarCategoriaProdutoRequest request, AtualizarCategoriaProdutoService service)
    {
        var command = new AtualizarCategoriaProdutoCommand
        {
            Id = id,
            Descricao = request.Descricao ?? string.Empty
        };

        await service.AtualizarCategoriaProduto(command);
        return Results.Ok();
    }

    private static async Task<IResult> ExcluirCategoriaProdutoAsync(Guid id, ExcluirCategoriaProdutoService service)
    {
        await service.ExcluirCategoriaProduto(new ExcluirCategoriaProdutoCommand { Id = id });
        return Results.NoContent();
    }

    private static async Task<IResult> ObterCategoriaProdutoAsync(Guid id, ObterCategoriaProdutoService service)
    {
        var categoria = await service.ObterCategoriaProduto(new ObterCategoriaProdutoQuery { Id = id });
        return Results.Ok(MapearCategoriaProdutoResponse(categoria));
    }

    private static async Task<IResult> ListarCategoriasProdutoAsync(ListarCategoriasProdutosService service)
    {
        var categorias = await service.ListarCategoriasProdutos(new ListarCategoriasProdutosQuery());
        return Results.Ok(categorias.Select(MapearCategoriaProdutoResponse).ToList());
    }

    private static CategoriaProdutoResponse MapearCategoriaProdutoResponse(CategoriaProduto categoria)
    {
        return new CategoriaProdutoResponse
        {
            Id = categoria.Id,
            Descricao = categoria.Descricao
        };
    }
}
