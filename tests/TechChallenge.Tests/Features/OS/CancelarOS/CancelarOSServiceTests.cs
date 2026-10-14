using FluentAssertions;
using Moq;
using TechChallenge.Application.Abstractions.Repositories;
using TechChallenge.Application.Features.OS.CancelarOS;
using TechChallenge.Domain.Entities;
using TechChallenge.Domain.Enums;

namespace TechChallenge.Tests.Features.OS.CancelarOS;

public class CancelarOSServiceTests
{
    [Fact]
    public async Task DeveCancelarOSQuandoTransicaoForValida()
    {
        var os = new OrdemServico (Guid.NewGuid(), "Descrição da OS", string.Empty, StatusOS.EmDiagnostico, Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), DateTime.Now, null, null, 0, 0, 0);
        var repository = new Mock<IOrdemServicoRepository>();
        repository.Setup(repo => repo.GetByIdAsync(os.Id)).ReturnsAsync(os);
        repository.Setup(repo => repo.UpdateAsync(os)).ReturnsAsync(os);
        var service = new CancelarOSService(repository.Object, new CancelarOSCommandValidator());

        var resultado = await service.CancelarOS(new CancelarOSCommand { OrdemServicoId = os.Id });

        resultado.Should().BeTrue();
        os.Status.Should().Be(StatusOS.Cancelada);
        os.DataAtualizacao.Should().NotBeNull();
        repository.Verify(repo => repo.UpdateAsync(os), Times.Once);
    }

    [Fact]
    public async Task DeveLancarQuandoOSNaoForEncontrada()
    {
        var osId = Guid.NewGuid();
        var repository = new Mock<IOrdemServicoRepository>();
        repository.Setup(repo => repo.GetByIdAsync(osId)).ReturnsAsync((OrdemServico?)null);
        var service = new CancelarOSService(repository.Object, new CancelarOSCommandValidator());

        var act = () => service.CancelarOS(new CancelarOSCommand { OrdemServicoId = osId });

        (await act.Should().ThrowAsync<KeyNotFoundException>())
            .WithMessage($"OS com Id {osId} não encontrada.");
        repository.Verify(repo => repo.UpdateAsync(It.IsAny<OrdemServico>()), Times.Never);
    }

    [Fact]
    public async Task DeveBloquearCancelamentoDeOSEntregue()
    {
        var os = new OrdemServico(Guid.NewGuid(), "Descrição da OS", string.Empty, StatusOS.Entregue, Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), DateTime.Now, null, null, 0, 0, 0);
        var repository = new Mock<IOrdemServicoRepository>();
        repository.Setup(repo => repo.GetByIdAsync(os.Id)).ReturnsAsync(os);
        var service = new CancelarOSService(repository.Object, new CancelarOSCommandValidator());

        var act = () => service.CancelarOS(new CancelarOSCommand { OrdemServicoId = os.Id });

        (await act.Should().ThrowAsync<InvalidOperationException>())
            .WithMessage($"Transição inválida: {StatusOS.Entregue} -> {StatusOS.Cancelada}.");
        repository.Verify(repo => repo.UpdateAsync(It.IsAny<OrdemServico>()), Times.Never);
    }
}
