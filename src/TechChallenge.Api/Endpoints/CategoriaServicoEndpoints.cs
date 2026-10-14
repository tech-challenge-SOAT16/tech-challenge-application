using TechChallenge.Api.Models.Request;
using TechChallenge.Api.Models.Response;
using TechChallenge.Application.Features.CategoriaServicos.AtualizarCategoriaServico;
using TechChallenge.Application.Features.CategoriaServicos.CriarCategoriaServico;
using TechChallenge.Application.Features.CategoriaServicos.ExcluirCategoriaServico;
using TechChallenge.Application.Features.CategoriaServicos.ListarCategoriasServicos;
using TechChallenge.Application.Features.CategoriaServicos.ObterCategoriaServico;
using TechChallenge.Domain.Entities;

namespace TechChallenge.Api.Endpoints;

public static class CategoriaServicoEndpoints
{
    public static IEndpointRouteBuilder MapCategoriaServicoEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/v1/categoriaservico")
            .WithName("CategoriaServico");

        group.MapPost("/", CriarCategoriaServicoAsync)
            .WithName("CriarCategoriaServico")
            .RequireAuthorization("AdminOuVendedor")
            .WithSummary("Cria uma nova categoria de veículo")
            .WithDescription("Adiciona uma nova categoria de veículo ao banco de dados")
            .Produces<Guid>(StatusCodes.Status201Created)
            .ProducesValidationProblem();

        group.MapGet("/{id}", ObterCategoriaServicoAsync)
            .WithName("ObterCategoriaServico")
            .WithSummary("Obtém uma categoria de veículo existente")
            .WithDescription("Obtém as informações de uma categoria de veículo existente do banco de dados")
            .Produces<CategoriaServicoResponse>()
            .Produces(StatusCodes.Status404NotFound);

        group.MapGet("/", ListarCategoriasServicoAsync)
            .WithName("ListarCategoriasServico")
            .WithSummary("Lista todas as categorias de veículo")
            .WithDescription("Lista todas as categorias de veículo do banco de dados")
            .Produces<List<CategoriaServicoResponse>>();

        group.MapPut("/{id}", AtualizarCategoriaServicoAsync)
            .WithName("AtualizarCategoriaServico")
            .RequireAuthorization("AdminOuVendedor")
            .WithSummary("Atualiza uma categoria de veículo existente")
            .WithDescription("Atualiza as informações de uma categoria de veículo existente no banco de dados")
            .ProducesValidationProblem();

        group.MapDelete("/{id}", ExcluirCategoriaServicoAsync)
            .WithName("ExcluirCategoriaServico")
            .RequireAuthorization("AdminOuVendedor")
            .WithSummary("Exclui uma categoria de veículo existente")
            .WithDescription("Exclui uma categoria de veículo existente do banco de dados")
            .ProducesValidationProblem();

        return app;
    }

    private static async Task<IResult> CriarCategoriaServicoAsync(CriarCategoriaServicoRequest request, CriarCategoriaServicoService service)
    {
        var command = new CriarCategoriaServicoCommand
        {
            Descricao = request.Descricao ?? string.Empty
        };

        var id = await service.CriarCategoriaServico(command);
        return Results.Created($"/api/v1/categoriaServico/{id}", id);
    }

    private static async Task<IResult> AtualizarCategoriaServicoAsync(Guid id, AtualizarCategoriaServicoRequest request, AtualizarCategoriaServicoService service)
    {
        var command = new AtualizarCategoriaServicoCommand
        {
            Id = id,
            Descricao = request.Descricao ?? string.Empty
        };

        await service.AtualizarCategoriaServico(command);
        return Results.Ok();
    }

    private static async Task<IResult> ExcluirCategoriaServicoAsync(Guid id, ExcluirCategoriaServicoService service)
    {
        await service.ExcluirCategoriaServico(new ExcluirCategoriaServicoCommand { Id = id });
        return Results.NoContent();
    }

    private static async Task<IResult> ObterCategoriaServicoAsync(Guid id, ObterCategoriaServicoService service)
    {
        var categoria = await service.ObterCategoriaServico(new ObterCategoriaServicoQuery { Id = id });
        return Results.Ok(MapearCategoriaServicoResponse(categoria));
    }

    private static async Task<IResult> ListarCategoriasServicoAsync(ListarCategoriasServicosService service)
    {
        var categorias = await service.ListarCategoriasServicos(new ListarCategoriasServicosQuery());
        return Results.Ok(categorias.Select(MapearCategoriaServicoResponse).ToList());
    }

    private static CategoriaServicoResponse MapearCategoriaServicoResponse(CategoriaServico categoria)
    {
        return new CategoriaServicoResponse
        {
            Id = categoria.Id,
            Descricao = categoria.Descricao
        };
    }
}