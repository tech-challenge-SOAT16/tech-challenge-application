using FluentAssertions;
using Moq;
using TechChallenge.Application.Abstractions.Repositories;
using TechChallenge.Application.Features.OS.ListarOS;
using TechChallenge.Domain.Entities;
using TechChallenge.Domain.Enums;

namespace TechChallenge.Tests.Features.OS.ListarOS;

public class ListarOSServiceTests
{
    [Fact]
    public async Task DeveListarTodasAsOSQuandoStatusNaoForInformado()
    {
        var ordens = new List<OrdemServico>
        {
            new(Guid.NewGuid(), "Descrição da OS", string.Empty, StatusOS.Recebida, Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), DateTime.Now, null, null, 0, 0, 0),
            new(Guid.NewGuid(), "Descrição da OS", string.Empty, StatusOS.Finalizada, Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), DateTime.Now, null, null, 0, 0, 0)
        };
        var repository = new Mock<IOrdemServicoRepository>();
        repository.Setup(repo => repo.GetAllAsync()).ReturnsAsync(ordens);
        var service = new ListarOSService(repository.Object);

        var resultado = await service.ListarOS(new ListarOSQuery());

        resultado.Should().BeEquivalentTo(ordens);
        repository.Verify(repo => repo.GetAllAsync(), Times.Once);
        repository.Verify(repo => repo.GetByStatusAsync(It.IsAny<StatusOS>()), Times.Never);
    }

    [Fact]
    public async Task DeveListarOSPorStatusQuandoStatusForInformado()
    {
        var ordens = new List<OrdemServico>
        {
            new(Guid.NewGuid(), "Descrição da OS", string.Empty, StatusOS.EmDiagnostico, Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), DateTime.Now, null, null, 0, 0, 0)
        };
        var repository = new Mock<IOrdemServicoRepository>();
        repository.Setup(repo => repo.GetByStatusAsync(StatusOS.EmDiagnostico)).ReturnsAsync(ordens);
        var service = new ListarOSService(repository.Object);

        var resultado = await service.ListarOS(new ListarOSQuery { Status = StatusOS.EmDiagnostico });

        resultado.Should().BeEquivalentTo(ordens);
        repository.Verify(repo => repo.GetByStatusAsync(StatusOS.EmDiagnostico), Times.Once);
        repository.Verify(repo => repo.GetAllAsync(), Times.Never);
    }
}
