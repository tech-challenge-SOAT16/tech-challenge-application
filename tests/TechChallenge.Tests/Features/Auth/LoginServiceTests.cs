using System;
using FluentAssertions;
using Moq;
using TechChallenge.Application.Abstractions.Auth;
using TechChallenge.Application.Abstractions.Repositories;
using TechChallenge.Application.Features.Auth.Login;
using TechChallenge.Domain.Entities;
using TechChallenge.Domain.Enums;

namespace TechChallenge.Tests.Features.Auth;

public class LoginServiceTests
{
    private readonly Mock<IUsuarioRepository> _usuarios = new();
    private readonly Mock<IRefreshTokenRepository> _refresh = new();
    private readonly Mock<IPasswordHasher> _hasher = new();
    private readonly Mock<ITokenService> _tokens = new();
    private readonly Mock<IAuthSettings> _settings = new();

    private LoginService CriarService()
    {
        _settings.SetupGet(s => s.RefreshTokenDays).Returns(7);
        return new LoginService(_usuarios.Object, _refresh.Object, _hasher.Object,
            _tokens.Object, _settings.Object, new LoginCommandValidator());
    }

    private static LoginCommand Comando() => new() { Login = "admin", Senha = "Senha@123" };

    [Fact]
    public async Task DeveLancarQuandoUsuarioNaoExiste()
    {
        _usuarios.Setup(r => r.GetByLoginAsync("admin")).ReturnsAsync((Usuario?)null);

        var acao = () => CriarService().Login(Comando());

        await acao.Should().ThrowAsync<UnauthorizedAccessException>();
    }

    [Fact]
    public async Task DeveLancarQuandoUsuarioInativo()
    {
        _usuarios.Setup(r => r.GetByLoginAsync("admin"))
            .ReturnsAsync(new Usuario(Guid.NewGuid(), "admin", "h", TipoUsuario.Administrador, true));

        var acao = () => CriarService().Login(Comando());

        await acao.Should().ThrowAsync<UnauthorizedAccessException>();
    }

    [Fact]
    public async Task DeveLancarQuandoSenhaInvalida()
    {
        _usuarios.Setup(r => r.GetByLoginAsync("admin"))
            .ReturnsAsync(new Usuario(Guid.NewGuid(), "admin", "h", TipoUsuario.Administrador, true));
        _hasher.Setup(h => h.Verify("Senha@123", "h")).Returns(false);

        var acao = () => CriarService().Login(Comando());

        await acao.Should().ThrowAsync<UnauthorizedAccessException>();
    }

    [Fact]
    public async Task DeveAutenticarEGerarRefreshTokenQuandoCredenciaisValidas()
    {
        var usuario = new Usuario(Guid.NewGuid(), "admin", "h", TipoUsuario.Administrador, true);

        _usuarios.Setup(r => r.GetByLoginAsync("admin")).ReturnsAsync(usuario);
        _hasher.Setup(h => h.Verify("Senha@123", "h")).Returns(true);
        _tokens.Setup(t => t.GerarAccessToken(usuario))
            .Returns(new AccessTokenResult("jwt", DateTime.UtcNow.AddMinutes(15)));
        _tokens.Setup(t => t.GerarRefreshToken()).Returns("cru");
        _tokens.Setup(t => t.HashRefreshToken("cru")).Returns("hash");
        RefreshToken? salvo = null;
        _refresh.Setup(r => r.AddAsync(It.IsAny<RefreshToken>()))
            .Callback<RefreshToken>(rt => salvo = rt)
            .ReturnsAsync((RefreshToken rt) => rt);

        var resultado = await CriarService().Login(Comando());

        resultado.AccessToken.Should().Be("jwt");
        resultado.RefreshToken.Should().Be("cru");
        salvo.Should().NotBeNull();
        salvo.TokenHash.Should().Be("hash");
        salvo.UsuarioId.Should().Be(usuario.Id);
        salvo.ExpiraEm.Should().BeAfter(salvo.CriadoEm);
        _refresh.Verify(r => r.AddAsync(It.IsAny<RefreshToken>()), Times.Once);
    }
}
