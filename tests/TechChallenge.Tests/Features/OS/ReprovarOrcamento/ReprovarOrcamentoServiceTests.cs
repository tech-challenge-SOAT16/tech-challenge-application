using FluentAssertions;
using Moq;
using TechChallenge.Application.Abstractions.Repositories;
using TechChallenge.Application.Features.OS.ReprovarOrcamento;
using TechChallenge.Domain.Entities;
using TechChallenge.Domain.Enums;

namespace TechChallenge.Tests.Features.OS.ReprovarOrcamento;

public class ReprovarOrcamentoServiceTests
{
    [Fact]
    public async Task DeveReprovarOrcamentoQuandoOSAguardarAprovacao()
    {
        var os = new OrdemServico(Guid.NewGuid(), "Descrição da OS", string.Empty, StatusOS.AguardandoAprovacao, Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), DateTime.Now, null, null, 0, 0, 0);
        var repository = new Mock<IOrdemServicoRepository>();
        repository.Setup(repo => repo.GetByIdAsync(os.Id)).ReturnsAsync(os);
        repository.Setup(repo => repo.UpdateAsync(os)).ReturnsAsync(os);
        var service = new ReprovarOrcamentoService(repository.Object, new ReprovarOrcamentoCommandValidator());

        var resultado = await service.ReprovarOrcamento(new ReprovarOrcamentoCommand { OrdemServicoId = os.Id });

        resultado.Should().BeTrue();
        os.Status.Should().Be(StatusOS.Reprovada);
        os.DataAtualizacao.Should().NotBeNull();
        repository.Verify(repo => repo.UpdateAsync(os), Times.Once);
    }

    [Fact]
    public async Task DeveBloquearReprovacaoForaDeAguardandoAprovacao()
    {
        var os = new OrdemServico(Guid.NewGuid(), string.Empty, string.Empty, StatusOS.EmDiagnostico, Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), DateTime.UtcNow, null, null, 0, 0, 0);
        var repository = new Mock<IOrdemServicoRepository>();
        repository.Setup(repo => repo.GetByIdAsync(os.Id)).ReturnsAsync(os);
        var service = new ReprovarOrcamentoService(repository.Object, new ReprovarOrcamentoCommandValidator());

        var act = () => service.ReprovarOrcamento(new ReprovarOrcamentoCommand { OrdemServicoId = os.Id });

        (await act.Should().ThrowAsync<InvalidOperationException>())
            .WithMessage($"Transição inválida: {StatusOS.EmDiagnostico} -> {StatusOS.Reprovada}.");
        repository.Verify(repo => repo.UpdateAsync(It.IsAny<OrdemServico>()), Times.Never);
    }
}
