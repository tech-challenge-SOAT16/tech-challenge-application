using System;
namespace TechChallenge.Api.Models.Response;

public record LoginResponse(
    string AccessToken,
    DateTime ExpiraEm,
    string RefreshToken,
    string TokenType);

public record UsuarioLogadoResponse(
    Guid UsuarioId,
    string Login,
    string TipoUsuario,
    Guid? FuncionarioId);

public record RefreshTokenResponse(
    Guid Id,
    DateTime CriadoEm,
    DateTime ExpiraEm);
