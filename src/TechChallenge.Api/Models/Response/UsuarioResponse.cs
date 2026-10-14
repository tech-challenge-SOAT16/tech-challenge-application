using System;
namespace TechChallenge.Api.Models.Response;

public record UsuarioResponse(
    Guid Id,
    string Login,
    string TipoUsuario,
    bool Ativo,
    Guid? FuncionarioId);
