using FluentAssertions;
using Moq;
using TechChallenge.Application.Abstractions.Repositories;
using TechChallenge.Application.Features.OS.EntregarOS;
using TechChallenge.Domain.Entities;
using TechChallenge.Domain.Enums;

namespace TechChallenge.Tests.Features.OS.EntregarOS;

public class EntregarOSServiceTests
{
    [Fact]
    public async Task DeveEntregarOSQuandoEstiverFinalizada()
    {
        var os = new OrdemServico(Guid.NewGuid(), "Descrição da OS", string.Empty, StatusOS.Finalizada, Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), DateTime.Now, null, null, 0, 0, 0);
        var repository = new Mock<IOrdemServicoRepository>();
        repository.Setup(repo => repo.GetByIdAsync(os.Id)).ReturnsAsync(os);
        repository.Setup(repo => repo.UpdateAsync(os)).ReturnsAsync(os);
        var service = new EntregarOSService(repository.Object, new EntregarOSCommandValidator());

        var resultado = await service.EntregarOS(new EntregarOSCommand { OrdemServicoId = os.Id });

        resultado.Should().BeTrue();
        os.Status.Should().Be(StatusOS.Entregue);
        os.DataAtualizacao.Should().NotBeNull();
        repository.Verify(repo => repo.UpdateAsync(os), Times.Once);
    }

    [Fact]
    public async Task DeveBloquearEntregaQuandoOSNaoEstiverFinalizada()
    {
        var os = new OrdemServico(Guid.NewGuid(), "Descrição da OS", string.Empty, StatusOS.EmExecucao, Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), DateTime.Now, null, null, 0, 0, 0);
        var repository = new Mock<IOrdemServicoRepository>();
        repository.Setup(repo => repo.GetByIdAsync(os.Id)).ReturnsAsync(os);
        var service = new EntregarOSService(repository.Object, new EntregarOSCommandValidator());

        var act = () => service.EntregarOS(new EntregarOSCommand { OrdemServicoId = os.Id });

        (await act.Should().ThrowAsync<InvalidOperationException>())
            .WithMessage($"Transição inválida: {StatusOS.EmExecucao} -> {StatusOS.Entregue}.");
        repository.Verify(repo => repo.UpdateAsync(It.IsAny<OrdemServico>()), Times.Never);
    }
}
