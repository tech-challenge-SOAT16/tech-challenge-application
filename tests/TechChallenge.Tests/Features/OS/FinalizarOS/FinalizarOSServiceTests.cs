using FluentAssertions;
using Moq;
using TechChallenge.Application.Abstractions.Notifications;
using TechChallenge.Application.Abstractions.Repositories;
using TechChallenge.Application.Features.OS.FinalizarOS;
using TechChallenge.Domain.Entities;
using TechChallenge.Domain.Enums;

namespace TechChallenge.Tests.Features.OS.FinalizarOS;

public class FinalizarOSServiceTests
{
    [Fact]
    public async Task DeveFinalizarOSQuandoEstiverEmExecucao()
    {
        var produto = new Produto(Guid.NewGuid(), "Filtro", 4, Guid.NewGuid());
        var os = new OrdemServico(Guid.NewGuid(), string.Empty, string.Empty, StatusOS.EmExecucao, Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), DateTime.UtcNow, null, null, 0, 0, 0);
        var osp = new OrdemServicoProdutos(Guid.NewGuid(), os.Id, produto.Id, 0, 2, 0, 0);
        os.AdicionarProdutos(osp);
        var repository = new Mock<IOrdemServicoRepository>();
        repository.Setup(repo => repo.GetByIdAsync(os.Id)).ReturnsAsync(os);
        repository.Setup(repo => repo.UpdateAsync(os)).ReturnsAsync(os);

        var estoqueRepository = new Mock<IEstoqueRepository>();
        var estoque = new TechChallenge.Domain.Entities.Estoque(Guid.NewGuid(), produto.Id, 2);
        estoqueRepository.Setup(repo => repo.GetByIdProdutoAsync(produto.Id)).ReturnsAsync(estoque);
        estoqueRepository.Setup(repo => repo.UpdateAsync(estoque)).ReturnsAsync(estoque);

        var service = new FinalizarOSService(repository.Object, estoqueRepository.Object, new FinalizarOSCommandValidator());

        var resultado = await service.FinalizarOS(new FinalizarOSCommand { OrdemServicoId = os.Id });

        resultado.Should().BeTrue();
        os.Status.Should().Be(StatusOS.Finalizada);
        os.DataFinalizacao.Should().NotBeNull();
        repository.Verify(repo => repo.UpdateAsync(os), Times.Once);
    }

    [Fact]
    public async Task DeveBloquearFinalizacaoQuandoEstoqueForInsuficiente()
    {
        var mecanicoId = Guid.NewGuid();
        
        var produto = new Produto(Guid.NewGuid(), "Filtro", 1, Guid.NewGuid());
        var os = new OrdemServico(Guid.NewGuid(), string.Empty, string.Empty, StatusOS.EmExecucao, Guid.NewGuid(), mecanicoId, Guid.NewGuid(), DateTime.UtcNow, null, null, 0, 0, 0);
        var osp = new OrdemServicoProdutos(Guid.NewGuid(), os.Id, produto.Id, 0, 2, 0, 0);
        osp.AdicionarProduto(produto.Id, produto.Descricao, produto.Valor, produto.CategoriaId);
        os.AdicionarProdutos(osp);

        var repository = new Mock<IOrdemServicoRepository>();
        var estoqueRepository = new Mock<IEstoqueRepository>();
        var notificationService = new Mock<INotificationService>();
        repository.Setup(repo => repo.GetByIdAsync(os.Id)).ReturnsAsync(os);
        var service = new FinalizarOSService(
            repository.Object,
            estoqueRepository.Object,
            new FinalizarOSCommandValidator(),
            notificationService.Object);

        var act = () => service.FinalizarOS(new FinalizarOSCommand { OrdemServicoId = os.Id });

        (await act.Should().ThrowAsync<InvalidOperationException>())
            .WithMessage("Estoque insuficiente para o produto Filtro.");
        notificationService.Verify(service => service.NotificarFuncionariosPorFuncao(
            TipoFuncionario.Administrador,
            "Estoque insuficiente",
            It.Is<string>(mensagem => mensagem.Contains(os.Id.ToString()) && mensagem.Contains("Filtro"))), Times.Once);
        notificationService.Verify(service => service.NotificarFuncionario(
            mecanicoId,
            "Estoque insuficiente",
            It.Is<string>(mensagem => mensagem.Contains(os.Id.ToString()) && mensagem.Contains("Filtro"))), Times.Once);
        repository.Verify(repo => repo.UpdateAsync(It.IsAny<OrdemServico>()), Times.Never);
    }

    [Fact]
    public async Task DeveBloquearFinalizacaoQuandoOSNaoEstiverEmExecucao()
    {
        var os = new OrdemServico(Guid.NewGuid(), string.Empty, string.Empty, StatusOS.AguardandoAprovacao, Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), DateTime.UtcNow, null, null, 0, 0, 0);
        var repository = new Mock<IOrdemServicoRepository>();
        repository.Setup(repo => repo.GetByIdAsync(os.Id)).ReturnsAsync(os);
        var estoqueRepository = new Mock<IEstoqueRepository>();
        var service = new FinalizarOSService(repository.Object, estoqueRepository.Object, new FinalizarOSCommandValidator());

        var act = () => service.FinalizarOS(new FinalizarOSCommand { OrdemServicoId = os.Id });

        (await act.Should().ThrowAsync<InvalidOperationException>())
            .WithMessage($"Transição inválida: {StatusOS.AguardandoAprovacao} -> {StatusOS.Finalizada}.");
        repository.Verify(repo => repo.UpdateAsync(It.IsAny<OrdemServico>()), Times.Never);
    }
}
