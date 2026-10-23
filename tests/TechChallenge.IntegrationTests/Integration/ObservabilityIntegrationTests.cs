using System.Collections.Concurrent;
using System.Diagnostics.Metrics;
using FluentAssertions;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using TechChallenge.Application.Abstractions.Observability;
using TechChallenge.Domain.Entities;
using TechChallenge.Domain.Enums;
using TechChallenge.Infrastructure.Database.Context;
using TechChallenge.Infrastructure.Observability;
using TechChallenge.IntegrationTests.Integration.Factories;

namespace TechChallenge.IntegrationTests.Integration;

public class ObservabilityIntegrationTests
{
    [Fact]
    public async Task DeveDevolverCorrelationIdRecebido()
    {
        using var factory = CreateFactory();
        using var client = factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            BaseAddress = new Uri("https://localhost")
        });
        using var request = new HttpRequestMessage(HttpMethod.Get, "/health/live");
        request.Headers.Add(CorrelationIdMiddleware.HeaderName, "demo-123");

        var response = await client.SendAsync(request);

        response.Headers.GetValues(CorrelationIdMiddleware.HeaderName).Should().ContainSingle("demo-123");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("valor invalido com espacos")]
    public async Task DeveGerarCorrelationIdQuandoAusenteOuInvalido(string? recebido)
    {
        using var factory = CreateFactory();
        using var client = factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            BaseAddress = new Uri("https://localhost")
        });
        using var request = new HttpRequestMessage(HttpMethod.Get, "/health/live");
        if (recebido is not null)
            request.Headers.TryAddWithoutValidation(CorrelationIdMiddleware.HeaderName, recebido);

        var response = await client.SendAsync(request);

        var correlationId = response.Headers.GetValues(CorrelationIdMiddleware.HeaderName).Single();
        Guid.TryParse(correlationId, out _).Should().BeTrue();
    }

    [Fact]
    public async Task DeveRegistrarTempoNoStatusAnteriorAoTransicionarOS()
    {
        var medicoes = new ConcurrentBag<(double Valor, string? Status)>();
        using var listener = new MeterListener();
        listener.InstrumentPublished = (instrument, meterListener) =>
        {
            if (instrument.Meter.Name == TechChallengeMetrics.MeterName && instrument.Name == "techchallenge.os.tempo_status")
                meterListener.EnableMeasurementEvents(instrument);
        };
        listener.SetMeasurementEventCallback<double>((_, valor, tags, _) =>
        {
            string? status = null;
            foreach (var tag in tags)
                if (tag.Key == "status")
                    status = tag.Value as string;
            medicoes.Add((valor, status));
        });
        listener.Start();

        await using var connection = new SqliteConnection("DataSource=:memory:");
        await connection.OpenAsync();
        var options = new DbContextOptionsBuilder<ApplicationDbContext>().UseSqlite(connection).Options;
        await using var context = new ApplicationDbContext(options);
        await context.Database.EnsureCreatedAsync();

        var endereco = new Endereco(Guid.NewGuid(), "Rua", "Casa", "1", "Centro", "Sao Paulo", "SP", "01001000");
        var cliente = new Cliente(Guid.NewGuid(), "Cliente", "cliente@teste.local", TipoDocumento.Cpf, "529.982.247-25", endereco.Id);
        cliente.AtribuirEndereco(endereco);
        var funcionario = new Funcionario(Guid.NewGuid(), "Mecanico", "529.982.247-25", "123456789", TipoFuncionario.Mecanico, endereco.Id);
        funcionario.AtribuirEndereco(endereco);
        var categoria = new CategoriaVeiculo(Guid.NewGuid(), "Passeio");
        var veiculo = new Veiculo(Guid.NewGuid(), "ABC1D23", "Modelo", "Marca", "Cor", 2020, 0, 0, cliente.Id, categoria.Id);
        var os = new OrdemServico(Guid.NewGuid(), "OS", Guid.NewGuid().ToString("N"), StatusOS.Recebida,
            cliente.Id, funcionario.Id, veiculo.Id, DateTime.UtcNow.AddHours(-2), null, null, 0, 0, 0);
        context.AddRange(endereco, cliente, funcionario, categoria, veiculo, os);
        await context.SaveChangesAsync();

        os.TransicionarPara(StatusOS.EmDiagnostico);
        await context.SaveChangesAsync();

        medicoes.Should().Contain(medicao =>
            medicao.Status == nameof(StatusOS.Recebida) &&
            medicao.Valor >= TimeSpan.FromHours(2).TotalSeconds - 60);
    }

    private static WebApplicationFactory<Program> CreateFactory() =>
        new WebAplicationFactory<Program>().WithWebHostBuilder(builder =>
            builder.ConfigureServices(services =>
                services.AddAuthentication(options =>
                {
                    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                })));
}
