using System.Data.Common;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using FluentAssertions;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Protocols;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Microsoft.IdentityModel.Tokens;
using TechChallenge.Domain.Entities;
using TechChallenge.Domain.Enums;
using TechChallenge.Infrastructure.Auth;
using TechChallenge.Infrastructure.Database.Context;

namespace TechChallenge.IntegrationTests.Integration;

/// <summary>
/// Usa a autenticação real (sem o esquema de teste que aprova tudo): token RS256
/// de cliente, como o emitido pela Function Serverless, e token HS256 de funcionário.
/// </summary>
public class AreaClienteAuthIntegrationTests : IClassFixture<AreaClienteAuthIntegrationTests.AuthFactory>
{
    private const string CustomerIssuer = "https://auth.test";
    private const string CustomerAudience = "tech-challenge-api";
    private const string EmployeeIssuer = "techchallenge-api";
    private const string EmployeeAudience = "techchallenge-clients";
    private const string EmployeeKey = "test-local-key-com-mais-de-32-caracteres-1234567890";

    private static readonly RSA CustomerRsa = RSA.Create(2048);
    private static readonly RsaSecurityKey CustomerKey = new(CustomerRsa) { KeyId = "test-kid" };

    private readonly AuthFactory _factory;

    public AreaClienteAuthIntegrationTests(AuthFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task ClienteDeveListarApenasAsPropriasOrdensDeServico()
    {
        var dados = await _factory.SeedAsync();
        using var client = CreateClient(CustomerToken(dados.ClienteA));

        var response = await client.GetAsync("/api/v1/cliente/ordens-servico");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var ordens = await response.Content.ReadFromJsonAsync<List<OrdemResumo>>();
        ordens!.Select(os => os.Id).Should().Equal(dados.OrdemA);
    }

    [Fact]
    public async Task ClienteNaoDeveVerOrdemDeServicoDeOutroCliente()
    {
        var dados = await _factory.SeedAsync();
        using var client = CreateClient(CustomerToken(dados.ClienteA));

        var propria = await client.GetAsync($"/api/v1/cliente/ordens-servico/{dados.OrdemA}");
        var deOutro = await client.GetAsync($"/api/v1/cliente/ordens-servico/{dados.OrdemB}");

        propria.StatusCode.Should().Be(HttpStatusCode.OK);
        deOutro.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Theory]
    [InlineData("/api/v1/clientes")]
    [InlineData("/api/v1/veiculos")]
    [InlineData("/api/v1/produtos")]
    [InlineData("/api/v1/ordens-servico")]
    public async Task TokenDeClienteNaoDeveAcessarRotasDeFuncionario(string rota)
    {
        var dados = await _factory.SeedAsync();
        using var client = CreateClient(CustomerToken(dados.ClienteA));

        var response = await client.GetAsync(rota);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task TokenDeClienteComAudienciaErradaDeveSerRecusado()
    {
        var dados = await _factory.SeedAsync();
        using var client = CreateClient(CustomerToken(dados.ClienteA, audience: "outra-api"));

        var response = await client.GetAsync("/api/v1/cliente/ordens-servico");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task TokenDeClienteSemEscopoCustomerDeveSerRecusado()
    {
        var dados = await _factory.SeedAsync();
        using var client = CreateClient(CustomerToken(dados.ClienteA, scope: "outro"));

        var response = await client.GetAsync("/api/v1/cliente/ordens-servico");

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task TokenDeFuncionarioNaoDeveAcessarAreaDoCliente()
    {
        await _factory.SeedAsync();
        using var client = CreateClient(EmployeeToken("Vendedor"));

        var response = await client.GetAsync("/api/v1/cliente/ordens-servico");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task RotasDeLeituraDeFuncionarioDevemExigirPerfil()
    {
        await _factory.SeedAsync();
        using var semPerfil = CreateClient(EmployeeToken(role: null));
        using var vendedor = CreateClient(EmployeeToken("Vendedor"));

        var negado = await semPerfil.GetAsync("/api/v1/clientes");
        var permitido = await vendedor.GetAsync("/api/v1/clientes");

        negado.StatusCode.Should().Be(HttpStatusCode.Forbidden);
        permitido.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    private HttpClient CreateClient(string token)
    {
        var client = _factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            BaseAddress = new Uri("https://localhost")
        });
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        return client;
    }

    private static string CustomerToken(Guid clienteId, string audience = CustomerAudience, string scope = "customer") =>
        new JsonWebTokenHandler().CreateToken(new SecurityTokenDescriptor
        {
            Issuer = CustomerIssuer,
            Audience = audience,
            Subject = new ClaimsIdentity([new Claim("sub", clienteId.ToString()), new Claim("scope", scope)]),
            Expires = DateTime.UtcNow.AddMinutes(10),
            SigningCredentials = new SigningCredentials(CustomerKey, SecurityAlgorithms.RsaSha256)
        });

    private static string EmployeeToken(string? role)
    {
        var claims = new List<Claim> { new("sub", Guid.NewGuid().ToString()), new("name", "funcionario") };
        if (role is not null)
            claims.Add(new Claim("role", role));

        return new JsonWebTokenHandler().CreateToken(new SecurityTokenDescriptor
        {
            Issuer = EmployeeIssuer,
            Audience = EmployeeAudience,
            Subject = new ClaimsIdentity(claims),
            Expires = DateTime.UtcNow.AddMinutes(10),
            SigningCredentials = new SigningCredentials(
                new SymmetricSecurityKey(Encoding.UTF8.GetBytes(EmployeeKey)), SecurityAlgorithms.HmacSha256)
        });
    }

    private sealed record OrdemResumo(Guid Id);

    public sealed record DadosSemeados(Guid ClienteA, Guid OrdemA, Guid OrdemB);

    public class AuthFactory : WebApplicationFactory<Program>
    {
        private DadosSemeados? _dados;
        private readonly SemaphoreSlim _lock = new(1, 1);

        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.ConfigureAppConfiguration(config => config.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Jwt:EnableLocalIssuance"] = "true",
                ["Jwt:LocalSigningKey"] = EmployeeKey,
                ["Jwt:Issuer"] = EmployeeIssuer,
                ["Jwt:Audience"] = EmployeeAudience,
                ["CustomerAuth:Enabled"] = "true",
                ["CustomerAuth:Authority"] = CustomerIssuer,
                ["CustomerAuth:Issuer"] = CustomerIssuer,
                ["CustomerAuth:Audience"] = CustomerAudience,
                ["ApprovalLinks:SigningKey"] = "test-approval-key-com-mais-de-32-caracteres-123456",
                ["IntegracaoExterna:ApiKey"] = "integration-test-key",
                ["Notifications:Enabled"] = "false"
            }));

            builder.ConfigureServices(services =>
            {
                SQLitePCL.Batteries.Init();

                // Sem rede nos testes: a chave pública do "emissor" é entregue diretamente.
                services.PostConfigure<JwtBearerOptions>(CustomerAuthOptions.SchemeName, options =>
                {
                    var configuration = new OpenIdConnectConfiguration { Issuer = CustomerIssuer };
                    configuration.SigningKeys.Add(new RsaSecurityKey(CustomerRsa.ExportParameters(false)) { KeyId = "test-kid" });
                    options.Configuration = configuration;
                    options.ConfigurationManager = new StaticConfigurationManager<OpenIdConnectConfiguration>(configuration);
                });

                services.RemoveAll<IDbContextOptionsConfiguration<ApplicationDbContext>>();
                services.RemoveAll<DbContextOptions<ApplicationDbContext>>();

                var connection = new SqliteConnection("DataSource=:memory:");
                connection.Open();
                services.AddSingleton<DbConnection>(connection);
                services.AddDbContext<ApplicationDbContext>((container, options) =>
                    options.UseSqlite(container.GetRequiredService<DbConnection>()));
            });

            builder.UseEnvironment("Testing");
        }

        public async Task<DadosSemeados> SeedAsync()
        {
            await _lock.WaitAsync();
            try
            {
                if (_dados is not null)
                    return _dados;

                using var scope = Services.CreateScope();
                var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                await context.Database.EnsureCreatedAsync();

                var endereco = new Endereco(Guid.NewGuid(), "Rua", "Casa", "1", "Centro", "Sao Paulo", "SP", "01001000");
                var clienteA = new Cliente(Guid.NewGuid(), "Cliente A", "a@teste.local", TipoDocumento.Cpf, "529.982.247-25", endereco.Id);
                clienteA.AtribuirEndereco(endereco);
                var clienteB = new Cliente(Guid.NewGuid(), "Cliente B", "b@teste.local", TipoDocumento.Cpf, "111.444.777-35", endereco.Id);
                clienteB.AtribuirEndereco(endereco);
                var funcionario = new Funcionario(Guid.NewGuid(), "Mecanico", "529.982.247-25", "123456789", TipoFuncionario.Mecanico, endereco.Id);
                funcionario.AtribuirEndereco(endereco);
                var categoria = new CategoriaVeiculo(Guid.NewGuid(), "Passeio");
                var veiculoA = new Veiculo(Guid.NewGuid(), "ABC1D23", "Modelo", "Marca", "Cor", 2020, 0, 0, clienteA.Id, categoria.Id);
                var veiculoB = new Veiculo(Guid.NewGuid(), "XYZ9K87", "Modelo", "Marca", "Cor", 2021, 0, 0, clienteB.Id, categoria.Id);
                var osA = NovaOS(clienteA.Id, funcionario.Id, veiculoA.Id);
                var osB = NovaOS(clienteB.Id, funcionario.Id, veiculoB.Id);

                context.AddRange(endereco, clienteA, clienteB, funcionario, categoria, veiculoA, veiculoB, osA, osB);
                await context.SaveChangesAsync();

                _dados = new DadosSemeados(clienteA.Id, osA.Id, osB.Id);
                return _dados;
            }
            finally
            {
                _lock.Release();
            }
        }

        private static OrdemServico NovaOS(Guid clienteId, Guid funcionarioId, Guid veiculoId) => new(
            Guid.NewGuid(), "OS", Guid.NewGuid().ToString("N"), StatusOS.Recebida,
            clienteId, funcionarioId, veiculoId, DateTime.UtcNow, null, null, 0, 0, 0);
    }
}
