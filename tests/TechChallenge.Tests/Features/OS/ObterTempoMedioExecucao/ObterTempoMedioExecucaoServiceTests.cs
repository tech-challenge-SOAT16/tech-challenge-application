using FluentAssertions;
using Moq;
using TechChallenge.Application.Abstractions.Repositories;
using TechChallenge.Application.Features.OS.ObterTempoMedioExecucao;
using TechChallenge.Domain.Entities;
using TechChallenge.Domain.Enums;

namespace TechChallenge.Tests.Features.OS.ObterTempoMedioExecucao;

public class ObterTempoMedioExecucaoServiceTests
{
    [Fact]
    public async Task DeveCalcularTempoMedioDasOrdensFinalizadas()
    {
        var inicio = new DateTime(2026, 1, 1, 8, 0, 0, DateTimeKind.Utc);
        var repository = new Mock<IOrdemServicoRepository>();
        repository.Setup(repo => repo.GetFinalizadasComDataFinalizacaoAsync())
            .ReturnsAsync(
            [
                new OrdemServico(Guid.NewGuid(), "Teste 1", "OS001", StatusOS.Recebida, Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), inicio, DateTime.UtcNow, inicio.AddHours(2), valor: 0, desconto: 0, acrescimo: 0),
                new OrdemServico(Guid.NewGuid(), "Teste 2", "OS002", StatusOS.Finalizada, Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), inicio, DateTime.UtcNow, inicio.AddHours(4), valor: 0, desconto: 0, acrescimo: 0)
            ]);
        var service = new ObterTempoMedioExecucaoService(repository.Object);

        var resultado = await service.ObterTempoMedioExecucao();

        resultado.QuantidadeOrdensFinalizadas.Should().Be(2);
        resultado.TempoMedioHoras.Should().Be(3);
        resultado.TempoMedioMinutos.Should().Be(180);
    }

    [Fact]
    public async Task DeveRetornarZeroQuandoNaoExistiremOrdensFinalizadas()
    {
        var repository = new Mock<IOrdemServicoRepository>();
        repository.Setup(repo => repo.GetFinalizadasComDataFinalizacaoAsync())
            .ReturnsAsync([]);
        var service = new ObterTempoMedioExecucaoService(repository.Object);

        var resultado = await service.ObterTempoMedioExecucao();

        resultado.QuantidadeOrdensFinalizadas.Should().Be(0);
        resultado.TempoMedioHoras.Should().Be(0);
        resultado.TempoMedioMinutos.Should().Be(0);
    }
}
