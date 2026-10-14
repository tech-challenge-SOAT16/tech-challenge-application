using System;
using FluentAssertions;
using Moq;
using TechChallenge.Application.Abstractions.Auth;
using TechChallenge.Application.Abstractions.Repositories;
using TechChallenge.Application.Features.Auth.Refresh;
using TechChallenge.Domain.Entities;
using TechChallenge.Domain.Enums;

namespace TechChallenge.Tests.Features.Auth;

public class RefreshServiceTests
{
    private readonly Mock<IRefreshTokenRepository> _refresh = new();
    private readonly Mock<ITokenService> _tokens = new();
    private readonly Mock<IAuthSettings> _settings = new();

    private RefreshService CriarService()
    {
        _settings.SetupGet(s => s.RefreshTokenDays).Returns(7);
        return new RefreshService(_refresh.Object, _tokens.Object, _settings.Object);
    }

    [Fact]
    public async Task DeveLancarQuandoTokenNaoExiste()
    {
        _tokens.Setup(t => t.HashRefreshToken("cru")).Returns("hash");
        _refresh.Setup(r => r.GetByHashAsync("hash")).ReturnsAsync((RefreshToken?)null);

        var acao = () => CriarService().Refresh(new RefreshCommand { RefreshToken = "cru" });

        await acao.Should().ThrowAsync<UnauthorizedAccessException>();
    }

    [Fact]
    public async Task DeveRotacionarQuandoTokenAtivo()
    {
        var usuario = new Usuario(Guid.NewGuid(), string.Empty, string.Empty, TipoUsuario.Vendedor, true);

        var agora = DateTime.UtcNow;
        var token = new RefreshToken(Guid.NewGuid(), usuario.Id, "hash", agora.AddMinutes(-5), agora.AddDays(7));
        token.AtribuirUsuario(usuario);

        _tokens.Setup(t => t.HashRefreshToken("cru")).Returns("hash");
        _tokens.Setup(t => t.HashRefreshToken("novo")).Returns("novo-hash");
        _refresh.Setup(r => r.GetByHashAsync("hash")).ReturnsAsync(token);
        _tokens.Setup(t => t.GerarAccessToken(usuario))
            .Returns(new AccessTokenResult("jwt", agora.AddMinutes(15)));
        _tokens.Setup(t => t.GerarRefreshToken()).Returns("novo");
        _refresh.Setup(r => r.AddAsync(It.IsAny<RefreshToken>())).ReturnsAsync((RefreshToken rt) => rt);
        _refresh.Setup(r => r.UpdateAsync(It.IsAny<RefreshToken>())).ReturnsAsync((RefreshToken rt) => rt);

        var resultado = await CriarService().Refresh(new RefreshCommand { RefreshToken = "cru" });

        resultado.RefreshToken.Should().Be("novo");
        token.RevogadoEm.Should().NotBeNull();
        _refresh.Verify(r => r.AddAsync(It.IsAny<RefreshToken>()), Times.Once);
        _refresh.Verify(r => r.UpdateAsync(token), Times.Once);
    }

    [Fact]
    public async Task DeveLancarQuandoTokenEstiverRevogado()
    {
        var agora = DateTime.UtcNow;
        var token = new RefreshToken(Guid.NewGuid(), Guid.NewGuid(), string.Empty, agora.AddDays(-1), agora.AddDays(7), agora.AddMinutes(-5));

        _tokens.Setup(t => t.HashRefreshToken("cru")).Returns("hash");
        _refresh.Setup(r => r.GetByHashAsync("hash")).ReturnsAsync(token);

        var acao = () => CriarService().Refresh(new RefreshCommand { RefreshToken = "cru" });

        await acao.Should().ThrowAsync<UnauthorizedAccessException>();
        _refresh.Verify(r => r.AddAsync(It.IsAny<RefreshToken>()), Times.Never);
    }
}
