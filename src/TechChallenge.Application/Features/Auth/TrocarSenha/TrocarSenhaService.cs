using System;
using FluentValidation;
using TechChallenge.Application.Abstractions.Auth;
using TechChallenge.Application.Abstractions.Repositories;

namespace TechChallenge.Application.Features.Auth.TrocarSenha;

public class TrocarSenhaService
{
    private readonly IUsuarioRepository _usuarioRepository;
    private readonly IRefreshTokenRepository _refreshTokenRepository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly ICurrentUser _currentUser;
    private readonly IValidator<TrocarSenhaCommand> _validator;

    public TrocarSenhaService(
        IUsuarioRepository usuarioRepository,
        IRefreshTokenRepository refreshTokenRepository,
        IPasswordHasher passwordHasher,
        ICurrentUser currentUser,
        IValidator<TrocarSenhaCommand> validator)
    {
        _usuarioRepository = usuarioRepository;
        _refreshTokenRepository = refreshTokenRepository;
        _passwordHasher = passwordHasher;
        _currentUser = currentUser;
        _validator = validator;
    }

    public async Task TrocarSenha(TrocarSenhaCommand command)
    {
        _validator.ValidateAndThrow(command);

        if (_currentUser.UsuarioId is not { } usuarioId)
            throw new UnauthorizedAccessException("Usuário não autenticado.");

        var usuario = await _usuarioRepository.GetByIdAsync(usuarioId);
        if (usuario is null)
            throw new UnauthorizedAccessException("Usuário não autenticado.");

        if (!_passwordHasher.Verify(command.SenhaAtual, usuario.PasswordHash))
            throw new UnauthorizedAccessException("Senha atual incorreta.");

        usuario = new Domain.Entities.Usuario(usuario.Id, usuario.Login, _passwordHasher.Hash(command.NovaSenha), usuario.TipoUsuario, usuario.Ativo, usuario.FuncionarioId);
        await _usuarioRepository.UpdateAsync(usuario);

        // Invalida todos os refresh tokens: força relogin com a nova senha.
        await _refreshTokenRepository.RevogarTodasDoUsuarioAsync(usuario.Id, DateTime.UtcNow);
    }
}
