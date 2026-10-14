using System;
using TechChallenge.Application.Abstractions.Auth;
using TechChallenge.Application.Abstractions.Repositories;

namespace TechChallenge.Application.Features.Auth.Logout;

public class LogoutService
{
    private readonly IRefreshTokenRepository _refreshTokenRepository;
    private readonly ICurrentUser _currentUser;

    public LogoutService(
        IRefreshTokenRepository refreshTokenRepository,
        ICurrentUser currentUser)
    {
        _refreshTokenRepository = refreshTokenRepository;
        _currentUser = currentUser;
    }

    public async Task Logout()
    {
        await RevogarTokensDoUsuarioAutenticado();
    }

    public async Task LogoutTodas()
    {
        await RevogarTokensDoUsuarioAutenticado();
    }

    private async Task RevogarTokensDoUsuarioAutenticado()
    {
        if (_currentUser.UsuarioId is not { } usuarioId)
            throw new UnauthorizedAccessException("Usuário não autenticado.");

        await _refreshTokenRepository.RevogarTodasDoUsuarioAsync(usuarioId, DateTime.UtcNow);
    }
}
