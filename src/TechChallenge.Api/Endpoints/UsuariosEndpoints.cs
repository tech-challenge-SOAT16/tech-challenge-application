using System;
using TechChallenge.Api.Models.Request;
using TechChallenge.Api.Models.Response;
using TechChallenge.Application.Features.Usuarios.AlterarStatus;
using TechChallenge.Application.Features.Usuarios.AlterarTipo;
using TechChallenge.Application.Features.Usuarios.CriarUsuario;
using TechChallenge.Application.Features.Usuarios.DesvincularFuncionario;
using TechChallenge.Application.Features.Usuarios.ListarUsuarios;
using TechChallenge.Application.Features.Usuarios.ObterUsuario;
using TechChallenge.Application.Features.Usuarios.ResetarSenha;
using TechChallenge.Application.Features.Usuarios.VincularFuncionario;
using TechChallenge.Domain.Entities;
using TechChallenge.Domain.Enums;

namespace TechChallenge.Api.Endpoints;

public static class UsuariosEndpoints
{
    public static IEndpointRouteBuilder MapUsuariosEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/v1/usuarios")
            .WithTags("Usuarios")
            .RequireAuthorization("AdminOnly");

        group.MapPost("/", CriarUsuarioAsync)
            .WithName("CriarUsuario")
            .WithSummary("Cria um novo usuário")
            .Produces<Guid>(StatusCodes.Status201Created)
            .ProducesValidationProblem();

        group.MapGet("/", ListarUsuariosAsync)
            .WithName("ListarUsuarios")
            .WithSummary("Lista os usuários")
            .Produces<List<UsuarioResponse>>();

        group.MapGet("/{id}", ObterUsuarioAsync)
            .WithName("ObterUsuario")
            .WithSummary("Obtém um usuário")
            .Produces<UsuarioResponse>()
            .Produces(StatusCodes.Status404NotFound);

        group.MapPatch("/{id}/tipo", AlterarTipoAsync)
            .WithName("AlterarTipoUsuario")
            .WithSummary("Altera o tipo (role) do usuário")
            .Produces(StatusCodes.Status204NoContent);

        group.MapPatch("/{id}/vincular-funcionario", VincularFuncionarioAsync)
            .WithName("VincularFuncionario")
            .WithSummary("Vincula o usuário a um funcionário")
            .Produces(StatusCodes.Status204NoContent);

        group.MapPatch("/{id}/desvincular-funcionario", DesvincularFuncionarioAsync)
            .WithName("DesvincularFuncionario")
            .WithSummary("Remove o vínculo do usuário com o funcionário")
            .Produces(StatusCodes.Status204NoContent);

        group.MapPatch("/{id}/ativar", AtivarUsuarioAsync)
            .WithName("AtivarUsuario")
            .WithSummary("Reativa o usuário")
            .Produces(StatusCodes.Status204NoContent);

        group.MapPatch("/{id}/desativar", DesativarUsuarioAsync)
            .WithName("DesativarUsuario")
            .WithSummary("Desativa o usuário e revoga os refresh tokens")
            .Produces(StatusCodes.Status204NoContent);

        group.MapPatch("/{id}/resetar-senha", ResetarSenhaAsync)
            .WithName("ResetarSenha")
            .WithSummary("Define uma nova senha e revoga os refresh tokens")
            .Produces(StatusCodes.Status204NoContent)
            .ProducesValidationProblem();

        return app;
    }

    private static async Task<IResult> CriarUsuarioAsync(CriarUsuarioRequest request, CriarUsuarioService service)
    {
        var command = new CriarUsuarioCommand
        {
            Login = request.Login,
            Senha = request.Senha,
            TipoUsuario = ParseTipo(request.TipoUsuario),
            FuncionarioId = request.FuncionarioId
        };

        var id = await service.CriarUsuario(command);
        return Results.Created($"/api/v1/usuarios/{id}", id);
    }

    private static async Task<IResult> ListarUsuariosAsync(ListarUsuariosService service)
    {
        var usuarios = (await service.ListarUsuarios()).Select(MapToResponse).ToList();
        return Results.Ok(usuarios);
    }

    private static async Task<IResult> ObterUsuarioAsync(Guid id, ObterUsuarioService service)
    {
        var usuario = await service.ObterUsuario(new ObterUsuarioQuery { Id = id });
        return Results.Ok(MapToResponse(usuario));
    }

    private static async Task<IResult> AlterarTipoAsync(Guid id, AlterarTipoRequest request, AlterarTipoService service)
    {
        await service.AlterarTipo(new AlterarTipoCommand { UsuarioId = id, TipoUsuario = ParseTipo(request.TipoUsuario) });
        return Results.NoContent();
    }

    private static async Task<IResult> VincularFuncionarioAsync(Guid id, VincularFuncionarioRequest request, VincularFuncionarioService service)
    {
        await service.VincularFuncionario(new VincularFuncionarioCommand { UsuarioId = id, FuncionarioId = request.FuncionarioId });
        return Results.NoContent();
    }

    private static async Task<IResult> DesvincularFuncionarioAsync(Guid id, DesvincularFuncionarioService service)
    {
        await service.DesvincularFuncionario(new DesvincularFuncionarioCommand { UsuarioId = id });
        return Results.NoContent();
    }

    private static async Task<IResult> AtivarUsuarioAsync(Guid id, AlterarStatusUsuarioService service)
    {
        await service.AlterarStatus(new AlterarStatusUsuarioCommand { UsuarioId = id, Ativo = true });
        return Results.NoContent();
    }

    private static async Task<IResult> DesativarUsuarioAsync(Guid id, AlterarStatusUsuarioService service)
    {
        await service.AlterarStatus(new AlterarStatusUsuarioCommand { UsuarioId = id, Ativo = false });
        return Results.NoContent();
    }

    private static async Task<IResult> ResetarSenhaAsync(Guid id, ResetarSenhaRequest request, ResetarSenhaService service)
    {
        await service.ResetarSenha(new ResetarSenhaCommand { UsuarioId = id, NovaSenha = request.NovaSenha });
        return Results.NoContent();
    }

    private static TipoUsuario ParseTipo(string tipo)
    {
        if (!Enum.TryParse<TipoUsuario>(tipo, true, out var resultado))
            throw new InvalidOperationException($"Tipo de usuário '{tipo}' inválido.");
        return resultado;
    }

    private static UsuarioResponse MapToResponse(Usuario usuario) =>
        new(usuario.Id, usuario.Login, usuario.TipoUsuario.ToString(), usuario.Ativo, usuario.FuncionarioId);
}
