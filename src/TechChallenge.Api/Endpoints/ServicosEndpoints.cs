using System;
using TechChallenge.Api.Models.Request;
using TechChallenge.Api.Models.Response;
using TechChallenge.Application.Features.Servicos.AtualizarServico;
using TechChallenge.Application.Features.Servicos.CriarServico;
using TechChallenge.Application.Features.Servicos.ExcluirServico;
using TechChallenge.Application.Features.Servicos.ListarServicos;
using TechChallenge.Application.Features.Servicos.ObterServico;

namespace TechChallenge.Api.Endpoints;

public static class ServicosEndpoints
{
    public static IEndpointRouteBuilder MapServicosEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/v1/servicos");

        group.MapPost("/", CriarServicoAsync)
            .WithName("CriarServico")
            .RequireAuthorization("AdminOuVendedor")
            .WithSummary("Cria um novo serviço")
            .WithDescription("Adiciona um novo serviço ao banco de dados")
            .Produces<Guid>(StatusCodes.Status201Created)
            .ProducesValidationProblem();

        group.MapGet("/{id}", ObterServicoAsync)
            .WithName("ObterServico")
            .WithSummary("Obtém um serviço existente")
            .WithDescription("Obtém as informações de um serviço existente do banco de dados")
            .Produces<ServicoResponse>()
            .Produces(StatusCodes.Status404NotFound);

        group.MapGet("/", ListarServicosAsync)
            .WithName("ListarServicos")
            .WithSummary("Lista todos os serviços")
            .WithDescription("Lista todos os serviços do banco de dados")
            .Produces<List<ServicoResponse>>();

        group.MapPut("/{id}", AtualizarServicoAsync)
            .WithName("AtualizarServico")
            .RequireAuthorization("AdminOuVendedor")
            .WithSummary("Atualiza um serviço existente")
            .WithDescription("Atualiza as informações de um serviço existente no banco de dados")
            .Produces(StatusCodes.Status200OK)
            .ProducesValidationProblem();

        group.MapDelete("/{id}", ExcluirServicoAsync)
            .WithName("ExcluirServico")
            .RequireAuthorization("AdminOuVendedor")
            .WithSummary("Exclui um serviço existente")
            .WithDescription("Exclui um serviço existente do banco de dados")
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status404NotFound);

        return app;
    }

    private static async Task<IResult> CriarServicoAsync(CriarServicoRequest request, CriarServicoService service)
    {
        var command = new CriarServicoCommand
        {
            Descricao = request.Descricao,
            Valor = request.Valor,
            CategoriaId = request.CategoriaId
        };

        var id = await service.CriarServico(command);
        return Results.Created($"/api/v1/servicos/{id}", id);
    }

    private static async Task<IResult> ObterServicoAsync(Guid id, ObterServicoService service)
    {
        var servico = await service.ObterServico(new ObterServicoQuery { Id = id });

        return Results.Ok(new ServicoResponse
        {
            Id = servico.Id,
            Descricao = servico.Descricao,
            Valor = servico.Valor
        });
    }

    private static async Task<IResult> ListarServicosAsync(ListarServicosService service)
    {
        var servicos = await service.ListarServicos(new ListarServicosQuery());

        var response = servicos.Select(s => new ServicoResponse
        {
            Id = s.Id,
            Descricao = s.Descricao,
            Valor = s.Valor
        }).ToList();

        return Results.Ok(response);
    }

    private static async Task<IResult> AtualizarServicoAsync(Guid id, AtualizarServicoRequest request, AtualizarServicoService service)
    {
        var command = new AtualizarServicoCommand
        {
            Id = id,
            Descricao = request.Descricao,
            Valor = request.Valor
        };

        await service.AtualizarServico(command);
        return Results.Ok();
    }

    private static async Task<IResult> ExcluirServicoAsync(Guid id, ExcluirServicoService service)
    {
        await service.ExcluirServico(new ExcluirServicoCommand { Id = id });
        return Results.NoContent();
    }
}
