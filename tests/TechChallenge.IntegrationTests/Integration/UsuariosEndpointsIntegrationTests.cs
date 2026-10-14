using System;
using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using TechChallenge.Api.Models.Request;
using TechChallenge.Api.Models.Response;
using TechChallenge.IntegrationTests.Integration.Factories;

namespace TechChallenge.IntegrationTests.Integration;

public class UsuariosEndpointsIntegrationTests : IClassFixture<WebAplicationFactory<Program>>
{
    private readonly HttpClient _client;

    public UsuariosEndpointsIntegrationTests(WebAplicationFactory<Program> factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task DeveCriarEListarUsuario()
    {
        var request = new CriarUsuarioRequest
        {
            Login = "vendedor.teste",
            Senha = "Senha@123",
            TipoUsuario = "Vendedor"
        };

        var criarResponse = await _client.PostAsJsonAsync("/api/v1/usuarios", request);
        var body = await criarResponse.Content.ReadAsStringAsync();
        criarResponse.StatusCode.Should().Be(HttpStatusCode.Created, body);

        var id = await criarResponse.Content.ReadFromJsonAsync<Guid>();
        id.Should().NotBeEmpty();

        var obterResponse = await _client.GetAsync($"/api/v1/usuarios/{id}");
        obterResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var usuario = await obterResponse.Content.ReadFromJsonAsync<UsuarioResponse>();
        usuario.Should().NotBeNull();
        usuario.Login.Should().Be("vendedor.teste");
        usuario.TipoUsuario.Should().Be("Vendedor");
        usuario.Ativo.Should().BeTrue();
    }

    [Fact]
    public async Task NaoMecanicoNaoPodeCriarUsuario()
    {
        var request = new CriarUsuarioRequest
        {
            Login = "bloqueado",
            Senha = "Senha@123",
            TipoUsuario = "Vendedor"
        };

        using var mensagem = new HttpRequestMessage(HttpMethod.Post, "/api/v1/usuarios")
        {
            Content = JsonContent.Create(request)
        };
        mensagem.Headers.Add(TestAuthHandler.RoleHeader, "Mecanico");

        var response = await _client.SendAsync(mensagem);

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task DeveAutenticarERotacionarRefreshToken()
    {
        var criar = new CriarUsuarioRequest
        {
            Login = "login.refresh",
            Senha = "Senha@123",
            TipoUsuario = "Administrador"
        };
        var criarResponse = await _client.PostAsJsonAsync("/api/v1/usuarios", criar);
        criarResponse.StatusCode.Should().Be(HttpStatusCode.Created);

        // Login real (endpoint anônimo) exercita LoginService + hasher + token JWT.
        var loginResponse = await _client.PostAsJsonAsync("/api/v1/auth/login",
            new LoginRequest { Login = "login.refresh", Senha = "Senha@123" });
        loginResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var login = await loginResponse.Content.ReadFromJsonAsync<LoginResponse>();
        login.Should().NotBeNull();
        login.AccessToken.Should().NotBeNullOrWhiteSpace();
        login.RefreshToken.Should().NotBeNullOrWhiteSpace();

        // Rotação: novo par, refresh diferente.
        var refreshResponse = await _client.PostAsJsonAsync("/api/v1/auth/refresh",
            new RefreshRequest { RefreshToken = login.RefreshToken });
        refreshResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var refreshed = await refreshResponse.Content.ReadFromJsonAsync<LoginResponse>();
        refreshed.Should().NotBeNull();
        refreshed.RefreshToken.Should().NotBe(login.RefreshToken);

        // Reuso do refresh antigo revogado -> 401.
        var reuseResponse = await _client.PostAsJsonAsync("/api/v1/auth/refresh",
            new RefreshRequest { RefreshToken = login.RefreshToken });
        reuseResponse.StatusCode.Should().Be(HttpStatusCode.Unauthorized);

        // Refresh novo continua válido, pois não há cadeia de sessão.
        var aposReuso = await _client.PostAsJsonAsync("/api/v1/auth/refresh",
            new RefreshRequest { RefreshToken = refreshed.RefreshToken });
        aposReuso.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task LoginComSenhaErradaRetornaUnauthorized()
    {
        var criar = new CriarUsuarioRequest
        {
            Login = "login.errado",
            Senha = "Senha@123",
            TipoUsuario = "Vendedor"
        };
        await _client.PostAsJsonAsync("/api/v1/usuarios", criar);

        var loginResponse = await _client.PostAsJsonAsync("/api/v1/auth/login",
            new LoginRequest { Login = "login.errado", Senha = "SenhaIncorreta1" });

        loginResponse.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}
