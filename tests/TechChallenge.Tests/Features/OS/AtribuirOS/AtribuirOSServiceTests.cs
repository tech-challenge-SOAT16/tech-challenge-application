using System;
using FluentAssertions;
using Moq;
using TechChallenge.Application.Abstractions.Repositories;
using TechChallenge.Application.Features.OS.AtribuirOS;
using TechChallenge.Domain.Entities;
using TechChallenge.Domain.Enums;

namespace TechChallenge.Tests.Features.OS.AtribuirOS;

public class AtribuirOSServiceTests
{
    [Fact]
    public async Task DeveAtribuirOSRecebidaParaMecanicoSemOSAtiva()
    {
        var mecanicoId = Guid.NewGuid();
        var ordemServico = new OrdemServico(Guid.NewGuid(), string.Empty, string.Empty, StatusOS.Recebida, Guid.NewGuid(), mecanicoId, Guid.NewGuid(), DateTime.UtcNow, null, null, 0, 0, 0);
        var funcionarioRepository = new Mock<IFuncionarioRepository>();
        var ordemServicoRepository = new Mock<IOrdemServicoRepository>();
        funcionarioRepository.Setup(repo => repo.GetByIdAsync(mecanicoId))
            .ReturnsAsync(new Funcionario(mecanicoId, string.Empty, string.Empty, string.Empty, TipoFuncionario.Mecanico, Guid.NewGuid()));
        ordemServicoRepository.Setup(repo => repo.GetOSAtivaMecanicoAsync(mecanicoId))
            .ReturnsAsync((OrdemServico?)null);
        ordemServicoRepository.Setup(repo => repo.GetByIdAsync(ordemServico.Id))
            .ReturnsAsync(ordemServico);
        ordemServicoRepository.Setup(repo => repo.UpdateAsync(It.IsAny<OrdemServico>()))
            .Returns<OrdemServico>(os => Task.FromResult(os));
        var service = new AtribuirOSService(
            ordemServicoRepository.Object,
            funcionarioRepository.Object,
            new AtribuirOSCommandValidator());

        var resultado = await service.AtribuirOS(new AtribuirOSCommand
        {
            OrdemServicoId = ordemServico.Id,
            MecanicoId = mecanicoId
        });

        resultado.Should().BeTrue();
        ordemServico.FuncionarioResponsavelId.Should().Be(mecanicoId);
        ordemServico.Status.Should().Be(StatusOS.EmDiagnostico);
        ordemServico.DataAtualizacao.Should().NotBeNull();
        ordemServicoRepository.Verify(repo => repo.UpdateAsync(ordemServico), Times.Once);
    }

    [Fact]
    public async Task DeveImpedirAtribuicaoQuandoMecanicoJaPossuirOSAtiva()
    {
        var mecanicoId = Guid.NewGuid();
        var osAtiva = new OrdemServico(Guid.NewGuid(), string.Empty, string.Empty, StatusOS.EmDiagnostico, Guid.NewGuid(), mecanicoId, Guid.NewGuid(), DateTime.UtcNow, null, null, 0, 0, 0);

        var funcionarioRepository = new Mock<IFuncionarioRepository>();
        var ordemServicoRepository = new Mock<IOrdemServicoRepository>();
        funcionarioRepository.Setup(repo => repo.GetByIdAsync(mecanicoId))
            .ReturnsAsync(new Funcionario(mecanicoId, string.Empty, string.Empty, string.Empty, TipoFuncionario.Mecanico, Guid.NewGuid()));
        ordemServicoRepository.Setup(repo => repo.GetOSAtivaMecanicoAsync(mecanicoId))
            .ReturnsAsync(osAtiva);
        var service = new AtribuirOSService(
            ordemServicoRepository.Object,
            funcionarioRepository.Object,
            new AtribuirOSCommandValidator());

        var act = () => service.AtribuirOS(new AtribuirOSCommand
        {
            OrdemServicoId = Guid.NewGuid(),
            MecanicoId = mecanicoId
        });

        (await act.Should().ThrowAsync<InvalidOperationException>())
            .WithMessage($"Mecânico já possui uma OS ativa (Id: {osAtiva.Id}).");
        ordemServicoRepository.Verify(repo => repo.UpdateAsync(It.IsAny<OrdemServico>()), Times.Never);
    }

    [Fact]
    public async Task DeveImpedirAtribuicaoQuandoOSNaoEstiverRecebida()
    {
        var mecanicoId = Guid.NewGuid();
        var ordemServico = new OrdemServico(Guid.NewGuid(), string.Empty, string.Empty, StatusOS.Finalizada, Guid.NewGuid(), mecanicoId, Guid.NewGuid(), DateTime.UtcNow, null, null, 0, 0, 0);
        var funcionarioRepository = new Mock<IFuncionarioRepository>();
        var ordemServicoRepository = new Mock<IOrdemServicoRepository>();
        funcionarioRepository.Setup(repo => repo.GetByIdAsync(mecanicoId))
            .ReturnsAsync(new Funcionario(mecanicoId, string.Empty, string.Empty, string.Empty, TipoFuncionario.Mecanico, Guid.NewGuid()));
        ordemServicoRepository.Setup(repo => repo.GetOSAtivaMecanicoAsync(mecanicoId))
            .ReturnsAsync((OrdemServico?)null);
        ordemServicoRepository.Setup(repo => repo.GetByIdAsync(ordemServico.Id))
            .ReturnsAsync(ordemServico);
        var service = new AtribuirOSService(
            ordemServicoRepository.Object,
            funcionarioRepository.Object,
            new AtribuirOSCommandValidator());

        var act = () => service.AtribuirOS(new AtribuirOSCommand
        {
            OrdemServicoId = ordemServico.Id,
            MecanicoId = mecanicoId
        });

        (await act.Should().ThrowAsync<InvalidOperationException>())
            .WithMessage($"Transição inválida: {ordemServico.Status} -> {StatusOS.EmDiagnostico}.");
        ordemServicoRepository.Verify(repo => repo.UpdateAsync(It.IsAny<OrdemServico>()), Times.Never);
    }
}
