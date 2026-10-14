using System;
using TechChallenge.Api.Models.Request;
using TechChallenge.Api.Models.Response;
using TechChallenge.Application.Features.Funcionarios.AtualizarFuncionario;
using TechChallenge.Application.Features.Funcionarios.CriarFuncionario;
using TechChallenge.Application.Features.Funcionarios.ExcluirFuncionario;
using TechChallenge.Application.Features.Funcionarios.ListarFuncionarios;
using TechChallenge.Application.Features.Funcionarios.ObterFuncionario;
using TechChallenge.Domain.Entities;

namespace TechChallenge.Api.Endpoints;

public static class FuncionariosEndpoints
{
    public static IEndpointRouteBuilder MapFuncionariosEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/v1/funcionarios")
            .WithName("Funcionarios")
            .RequireAuthorization("AdminOnly");

        group.MapPost("/", CriarFuncionarioAsync)
            .WithName("CriarFuncionario")
            .WithSummary("Cria um novo funcionário")
            .WithDescription("Adiciona um novo funcionário ao banco de dados")
            .Produces<Guid>(StatusCodes.Status201Created)
            .ProducesValidationProblem();

        group.MapGet("/{id}", ObterFuncionarioAsync)
            .WithName("ObterFuncionario")
            .WithSummary("Obtém um funcionário existente")
            .WithDescription("Obtém as informações de um funcionário existente do banco de dados")
            .Produces<FuncionarioResponse>()
            .Produces(StatusCodes.Status404NotFound);

        group.MapGet("/", ListarFuncionariosAsync)
            .WithName("ListarFuncionarios")
            .WithSummary("Lista todos os funcionários")
            .WithDescription("Lista todos os funcionários do banco de dados")
            .Produces<List<FuncionarioResponse>>();

        group.MapPut("/{id}", AtualizarFuncionarioAsync)
            .WithName("AtualizarFuncionario")
            .WithSummary("Atualiza um funcionário existente")
            .WithDescription("Atualiza as informações de um funcionário existente no banco de dados")
            .ProducesValidationProblem();

        group.MapDelete("/{id}", ExcluirFuncionarioAsync)
            .WithName("ExcluirFuncionario")
            .WithSummary("Exclui um funcionário existente")
            .WithDescription("Exclui um funcionário existente do banco de dados")
            .ProducesValidationProblem();

        return app;
    }

    private static async Task<IResult> CriarFuncionarioAsync(CriarFuncionarioRequest request, CriarFuncionarioService service)
    {
        var endereco = request.Endereco ?? new EnderecoRequest();

        var command = new CriarFuncionarioCommand
        {
            Nome = request.Nome,
            Cpf = request.Cpf,
            Rg = request.Rg,
            Cargo = request.Cargo.ToString(),
            Logradouro = endereco.Logradouro ?? string.Empty,
            Complemento = endereco.Complemento ?? string.Empty,
            Numero = endereco.Numero ?? string.Empty,
            Bairro = endereco.Bairro ?? string.Empty,
            Cidade = endereco.Cidade ?? string.Empty,
            Estado = endereco.Estado ?? string.Empty,
            Cep = endereco.Cep ?? string.Empty
        };

        var id = await service.CriarFuncionario(command);
        return Results.Created($"/api/v1/funcionarios/{id}", id);
    }

    private static async Task<IResult> ObterFuncionarioAsync(Guid id, ObterFuncionarioService service)
    {
        var funcionario = await service.ObterFuncionario(new ObterFuncionarioQuery { Id = id });
        return Results.Ok(MapearFuncionarioResponse(funcionario));
    }

    private static async Task<IResult> ListarFuncionariosAsync(ListarFuncionariosService service)
    {
        var funcionarios = await service.ListarFuncionarios(new ListarFuncionariosQuery());
        return Results.Ok(funcionarios.Select(MapearFuncionarioResponse).ToList());
    }

    private static async Task<IResult> AtualizarFuncionarioAsync(Guid id, AtualizarFuncionarioRequest request, AtualizarFuncionarioService service)
    {
        var endereco = request.Endereco ?? new EnderecoRequest();

        var command = new AtualizarFuncionarioCommand
        {
            Id = id,
            Nome = request.Nome,
            Cpf = request.Cpf,
            Rg = request.Rg,
            Cargo = request.Cargo,
            Logradouro = endereco.Logradouro ?? string.Empty,
            Complemento = endereco.Complemento ?? string.Empty,
            Numero = endereco.Numero ?? string.Empty,
            Bairro = endereco.Bairro ?? string.Empty,
            Cidade = endereco.Cidade ?? string.Empty,
            Estado = endereco.Estado ?? string.Empty,
            Cep = endereco.Cep ?? string.Empty
        };

        await service.AtualizarFuncionario(command);
        return Results.Ok();
    }

    private static async Task<IResult> ExcluirFuncionarioAsync(Guid id, ExcluirFuncionarioService service)
    {
        await service.ExcluirFuncionario(new ExcluirFuncionarioCommand { Id = id });
        return Results.NoContent();
    }

    private static FuncionarioResponse MapearFuncionarioResponse(Funcionario funcionario)
    {
        return new FuncionarioResponse
        {
            Id = funcionario.Id,
            Nome = funcionario.Nome,
            Cpf = funcionario.Cpf,
            Rg = funcionario.Rg,
            Cargo = funcionario.TipoFuncionario.ToString(),
            Endereco = new EnderecoResponse
            {
                Id = funcionario.Endereco.Id,
                Logradouro = funcionario.Endereco.Logradouro,
                Complemento = funcionario.Endereco.Complemento,
                Numero = funcionario.Endereco.Numero,
                Bairro = funcionario.Endereco.Bairro,
                Cidade = funcionario.Endereco.Cidade,
                Estado = funcionario.Endereco.Estado,
                Cep = funcionario.Endereco.Cep
            }
        };
    }
}
