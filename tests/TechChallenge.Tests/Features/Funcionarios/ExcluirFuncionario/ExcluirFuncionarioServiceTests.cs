using FluentAssertions;
using Moq;
using TechChallenge.Application.Abstractions.Repositories;
using TechChallenge.Application.Features.Funcionarios.ExcluirFuncionario;
using TechChallenge.Domain.Entities;
using TechChallenge.Domain.Enums;

namespace TechChallenge.Tests.Features.Funcionarios.ExcluirFuncionario;

public class ExcluirFuncionarioServiceTests
{
    [Fact]
    public async Task DeveExcluirFuncionarioQuandoNaoPossuirOrdemServico()
    {
        var funcionario = new Funcionario(Guid.NewGuid(), string.Empty, string.Empty, string.Empty, TipoFuncionario.Mecanico, Guid.NewGuid());
        var funcionarioRepository = new Mock<IFuncionarioRepository>();
        var ordemServicoRepository = new Mock<IOrdemServicoRepository>();
        funcionarioRepository.Setup(repo => repo.GetByIdAsync(funcionario.Id)).ReturnsAsync(funcionario);
        ordemServicoRepository.Setup(repo => repo.ExistePorFuncionarioAsync(funcionario.Id)).ReturnsAsync(false);
        funcionarioRepository.Setup(repo => repo.DeleteAsync(funcionario)).Returns(Task.CompletedTask);
        var service = new ExcluirFuncionarioService(
            funcionarioRepository.Object,
            ordemServicoRepository.Object,
            new ExcluirFuncionarioCommandValidator());

        var resultado = await service.ExcluirFuncionario(new ExcluirFuncionarioCommand { Id = funcionario.Id });

        resultado.Should().BeTrue();
        funcionarioRepository.Verify(repo => repo.DeleteAsync(funcionario), Times.Once);
    }

    [Fact]
    public async Task DeveImpedirExclusaoQuandoFuncionarioPossuirOrdemServico()
    {
        var funcionario = new Funcionario(Guid.NewGuid(), string.Empty, string.Empty, string.Empty, TipoFuncionario.Mecanico, Guid.NewGuid());
        var funcionarioRepository = new Mock<IFuncionarioRepository>();
        var ordemServicoRepository = new Mock<IOrdemServicoRepository>();
        funcionarioRepository.Setup(repo => repo.GetByIdAsync(funcionario.Id)).ReturnsAsync(funcionario);
        ordemServicoRepository.Setup(repo => repo.ExistePorFuncionarioAsync(funcionario.Id)).ReturnsAsync(true);
        var service = new ExcluirFuncionarioService(
            funcionarioRepository.Object,
            ordemServicoRepository.Object,
            new ExcluirFuncionarioCommandValidator());

        var act = () => service.ExcluirFuncionario(new ExcluirFuncionarioCommand { Id = funcionario.Id });

        (await act.Should().ThrowAsync<InvalidOperationException>())
            .WithMessage("Não é possível excluir um funcionário associado a uma ordem de serviço.");
        funcionarioRepository.Verify(repo => repo.DeleteAsync(It.IsAny<Funcionario>()), Times.Never);
    }
}
