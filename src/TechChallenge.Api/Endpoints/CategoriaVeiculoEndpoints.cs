using TechChallenge.Api.Models.Request;
using TechChallenge.Api.Models.Response;
using TechChallenge.Application.Features.CategoriaVeiculos.AtualizarCategoriaVeiculo;
using TechChallenge.Application.Features.CategoriaVeiculos.CriarCategoriaVeiculo;
using TechChallenge.Application.Features.CategoriaVeiculos.ExcluirCategoriaVeiculo;
using TechChallenge.Application.Features.CategoriaVeiculos.ListarCategoriasVeiculos;
using TechChallenge.Application.Features.CategoriaVeiculos.ObterCategoriaVeiculo;
using TechChallenge.Domain.Entities;

namespace TechChallenge.Api.Endpoints;

public static class CategoriaVeiculoEndpoints
{
    public static IEndpointRouteBuilder MapCategoriaVeiculoEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/v1/categoriaveiculo")
            .WithName("CategoriaVeiculo");

        group.MapPost("/", CriarCategoriaVeiculoAsync)
            .WithName("CriarCategoriaVeiculo")
            .RequireAuthorization("AdminOuVendedor")
            .WithSummary("Cria uma nova categoria de veículo")
            .WithDescription("Adiciona uma nova categoria de veículo ao banco de dados")
            .Produces<Guid>(StatusCodes.Status201Created)
            .ProducesValidationProblem();

        group.MapGet("/{id}", ObterCategoriaVeiculoAsync)
            .WithName("ObterCategoriaVeiculo")
            .WithSummary("Obtém uma categoria de veículo existente")
            .WithDescription("Obtém as informações de uma categoria de veículo existente do banco de dados")
            .Produces<CategoriaVeiculoResponse>()
            .Produces(StatusCodes.Status404NotFound);

        group.MapGet("/", ListarCategoriasVeiculoAsync)
            .WithName("ListarCategoriasVeiculo")
            .WithSummary("Lista todas as categorias de veículo")
            .WithDescription("Lista todas as categorias de veículo do banco de dados")
            .Produces<List<CategoriaVeiculoResponse>>();

        group.MapPut("/{id}", AtualizarCategoriaVeiculoAsync)
            .WithName("AtualizarCategoriaVeiculo")
            .RequireAuthorization("AdminOuVendedor")
            .WithSummary("Atualiza uma categoria de veículo existente")
            .WithDescription("Atualiza as informações de uma categoria de veículo existente no banco de dados")
            .ProducesValidationProblem();

        group.MapDelete("/{id}", ExcluirCategoriaVeiculoAsync)
            .WithName("ExcluirCategoriaVeiculo")
            .RequireAuthorization("AdminOuVendedor")
            .WithSummary("Exclui uma categoria de veículo existente")
            .WithDescription("Exclui uma categoria de veículo existente do banco de dados")
            .ProducesValidationProblem();

        return app;
    }

    private static async Task<IResult> CriarCategoriaVeiculoAsync(CriarCategoriaVeiculoRequest request, CriarCategoriaVeiculoService service)
    {
        var command = new CriarCategoriaVeiculoCommand
        {
            Descricao = request.Descricao ?? string.Empty
        };

        var id = await service.CriarCategoriaVeiculo(command);
        return Results.Created($"/api/v1/categoriaveiculo/{id}", id);
    }

    private static async Task<IResult> AtualizarCategoriaVeiculoAsync(Guid id, AtualizarCategoriaVeiculoRequest request, AtualizarCategoriaVeiculoService service)
    {
        var command = new AtualizarCategoriaVeiculoCommand
        {
            Id = id,
            Descricao = request.Descricao ?? string.Empty
        };

        await service.AtualizarCategoriaVeiculo(command);
        return Results.Ok();
    }

    private static async Task<IResult> ExcluirCategoriaVeiculoAsync(Guid id, ExcluirCategoriaVeiculoService service)
    {
        await service.ExcluirCategoriaVeiculo(new ExcluirCategoriaVeiculoCommand { Id = id });
        return Results.NoContent();
    }

    private static async Task<IResult> ObterCategoriaVeiculoAsync(Guid id, ObterCategoriaVeiculoService service)
    {
        var categoria = await service.ObterCategoriaVeiculo(new ObterCategoriaVeiculoQuery { Id = id });
        return Results.Ok(MapearCategoriaVeiculoResponse(categoria));
    }

    private static async Task<IResult> ListarCategoriasVeiculoAsync(ListarCategoriasVeiculosService service)
    {
        var categorias = await service.ListarCategoriasVeiculos(new ListarCategoriasVeiculosQuery());
        return Results.Ok(categorias.Select(MapearCategoriaVeiculoResponse).ToList());
    }

    private static CategoriaVeiculoResponse MapearCategoriaVeiculoResponse(CategoriaVeiculo categoria)
    {
        return new CategoriaVeiculoResponse
        {
            Id = categoria.Id,
            Descricao = categoria.Descricao
        };
    }
}