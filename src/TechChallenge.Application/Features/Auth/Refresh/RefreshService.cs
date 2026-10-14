using System;
using TechChallenge.Application.Abstractions.Auth;
using TechChallenge.Application.Abstractions.Repositories;
using TechChallenge.Domain.Entities;

namespace TechChallenge.Application.Features.Auth.Refresh;

public class RefreshService
{
    private readonly IRefreshTokenRepository _refreshTokenRepository;
    private readonly ITokenService _tokenService;
    private readonly IAuthSettings _settings;

    public RefreshService(
        IRefreshTokenRepository refreshTokenRepository,
        ITokenService tokenService,
        IAuthSettings settings)
    {
        _refreshTokenRepository = refreshTokenRepository;
        _tokenService = tokenService;
        _settings = settings;
    }

    public async Task<AuthResult> Refresh(RefreshCommand command)
    {
        if (string.IsNullOrWhiteSpace(command.RefreshToken))
            throw new UnauthorizedAccessException("Refresh token inválido.");

        var agora = DateTime.UtcNow;
        var hash = _tokenService.HashRefreshToken(command.RefreshToken);
        var token = await _refreshTokenRepository.GetByHashAsync(hash);

        if (token is null)
            throw new UnauthorizedAccessException("Refresh token inválido.");

        if (!token.EstaAtivo(agora))
        {
            throw new UnauthorizedAccessException("Refresh token expirado.");
        }

        var usuario = token.Usuario;
        if (usuario is null || !usuario.Ativo)
            throw new UnauthorizedAccessException("Credenciais inválidas.");

        var access = _tokenService.GerarAccessToken(usuario);
        var novoCru = _tokenService.GerarRefreshToken();

        var expira = agora.AddDays(_settings.RefreshTokenDays);
        var novo = new RefreshToken(Guid.NewGuid(), usuario.Id, _tokenService.HashRefreshToken(novoCru), agora, expira);

        token.AlterarRevogacao(agora);
        await _refreshTokenRepository.AddAsync(novo);
        await _refreshTokenRepository.UpdateAsync(token);

        return new AuthResult(access.Token, access.ExpiraEm, novoCru);
    }
}
