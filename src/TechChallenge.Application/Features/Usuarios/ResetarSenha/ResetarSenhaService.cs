using System;
using FluentValidation;
using TechChallenge.Application.Abstractions.Auth;
using TechChallenge.Application.Abstractions.Repositories;

namespace TechChallenge.Application.Features.Usuarios.ResetarSenha;

public class ResetarSenhaService
{
    private readonly IUsuarioRepository _usuarioRepository;
    private readonly IRefreshTokenRepository _refreshTokenRepository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IValidator<ResetarSenhaCommand> _validator;

    public ResetarSenhaService(
        IUsuarioRepository usuarioRepository,
        IRefreshTokenRepository refreshTokenRepository,
        IPasswordHasher passwordHasher,
        IValidator<ResetarSenhaCommand> validator)
    {
        _usuarioRepository = usuarioRepository;
        _refreshTokenRepository = refreshTokenRepository;
        _passwordHasher = passwordHasher;
        _validator = validator;
    }

    public async Task ResetarSenha(ResetarSenhaCommand command)
    {
        _validator.ValidateAndThrow(command);

        var usuario = await _usuarioRepository.GetByIdAsync(command.UsuarioId);
        if (usuario is null)
            throw new KeyNotFoundException($"Usuário com Id {command.UsuarioId} não encontrado.");

        usuario = new Domain.Entities.Usuario(usuario.Id, usuario.Login, _passwordHasher.Hash(command.NovaSenha), usuario.TipoUsuario, usuario.Ativo, usuario.FuncionarioId);
        await _usuarioRepository.UpdateAsync(usuario);

        await _refreshTokenRepository.RevogarTodasDoUsuarioAsync(usuario.Id, DateTime.UtcNow);
    }
}
