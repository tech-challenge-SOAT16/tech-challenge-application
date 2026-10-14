using System;
using FluentValidation;
using TechChallenge.Application.Abstractions.Auth;
using TechChallenge.Application.Abstractions.Repositories;
using TechChallenge.Domain.Entities;

namespace TechChallenge.Application.Features.Auth.Login;

public class LoginService
{
    private readonly IUsuarioRepository _usuarioRepository;
    private readonly IRefreshTokenRepository _refreshTokenRepository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly ITokenService _tokenService;
    private readonly IAuthSettings _settings;
    private readonly IValidator<LoginCommand> _validator;

    public LoginService(
        IUsuarioRepository usuarioRepository,
        IRefreshTokenRepository refreshTokenRepository,
        IPasswordHasher passwordHasher,
        ITokenService tokenService,
        IAuthSettings settings,
        IValidator<LoginCommand> validator)
    {
        _usuarioRepository = usuarioRepository;
        _refreshTokenRepository = refreshTokenRepository;
        _passwordHasher = passwordHasher;
        _tokenService = tokenService;
        _settings = settings;
        _validator = validator;
    }

    public async Task<AuthResult> Login(LoginCommand command)
    {
        _validator.ValidateAndThrow(command);

        var usuario = await _usuarioRepository.GetByLoginAsync(command.Login);
        if (usuario is null || !usuario.Ativo)
            throw new UnauthorizedAccessException("Credenciais inválidas.");

        if (!_passwordHasher.Verify(command.Senha, usuario.PasswordHash))
            throw new UnauthorizedAccessException("Credenciais inválidas.");

        var access = _tokenService.GerarAccessToken(usuario);
        var refreshCru = _tokenService.GerarRefreshToken();

        var agora = DateTime.UtcNow;
        var refreshToken = new RefreshToken(Guid.NewGuid(), usuario.Id, _tokenService.HashRefreshToken(refreshCru), agora, agora.AddDays(_settings.RefreshTokenDays));

        await _refreshTokenRepository.AddAsync(refreshToken);

        return new AuthResult(access.Token, access.ExpiraEm, refreshCru);
    }
}
