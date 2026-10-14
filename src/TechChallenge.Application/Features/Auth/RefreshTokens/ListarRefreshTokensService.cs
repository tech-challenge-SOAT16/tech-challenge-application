using System;
using TechChallenge.Application.Abstractions.Auth;
using TechChallenge.Application.Abstractions.Repositories;

namespace TechChallenge.Application.Features.Auth.RefreshTokens;

public class ListarRefreshTokensService
{
    private readonly IRefreshTokenRepository _refreshTokenRepository;
    private readonly ICurrentUser _currentUser;

    public ListarRefreshTokensService(IRefreshTokenRepository refreshTokenRepository, ICurrentUser currentUser)
    {
        _refreshTokenRepository = refreshTokenRepository;
        _currentUser = currentUser;
    }

    public async Task<List<RefreshTokenDto>> ListarRefreshTokens()
    {
        if (_currentUser.UsuarioId is not { } usuarioId)
            throw new UnauthorizedAccessException("Usuário não autenticado.");

        var tokens = await _refreshTokenRepository.GetAtivasDoUsuarioAsync(usuarioId, DateTime.UtcNow);

        return tokens
            .Select(t => new RefreshTokenDto(t.Id, t.CriadoEm, t.ExpiraEm))
            .ToList();
    }
}
