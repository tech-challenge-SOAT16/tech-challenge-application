using FluentAssertions;
using Moq;
using TechChallenge.Application.Abstractions.Notifications;
using TechChallenge.Application.Abstractions.Repositories;
using TechChallenge.Application.Features.OS.EnviarOrcamento;
using TechChallenge.Domain.Entities;
using TechChallenge.Domain.Enums;

namespace TechChallenge.Tests.Features.OS.EnviarOrcamento;

public class EnviarOrcamentoServiceTests
{
    [Fact]
    public async Task DeveCalcularOrcamentoEEnviarQuandoOSTiverItens()
    {
        var os = new OrdemServico(Guid.NewGuid(), string.Empty, string.Empty, StatusOS.EmDiagnostico, Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), DateTime.UtcNow, null, null, 0, 0, 0);
        
        os.AdicionarServicos(new OrdemServicoServicos(Guid.NewGuid(), os.Id, Guid.NewGuid(), 100, 2, 5, 10));
        var produtoId = Guid.NewGuid();
        os.AdicionarProdutos(new OrdemServicoProdutos(Guid.NewGuid(), os.Id, produtoId, 50, 3, 0, 0));

        var repository = new Mock<IOrdemServicoRepository>();
        var estoqueRepository = new Mock<IEstoqueRepository>();
        estoqueRepository.Setup(repo => repo.GetByIdProdutoAsync(produtoId))
            .ReturnsAsync(new TechChallenge.Domain.Entities.Estoque(Guid.NewGuid(), produtoId, 3));
        repository.Setup(repo => repo.GetByIdAsync(os.Id)).ReturnsAsync(os);
        repository.Setup(repo => repo.UpdateAsync(os)).ReturnsAsync(os);
        var service = new EnviarOrcamentoService(repository.Object, estoqueRepository.Object, new EnviarOrcamentoCommandValidator());

        var resultado = await service.EnviarOrcamento(new EnviarOrcamentoCommand { OrdemServicoId = os.Id });

        resultado.Should().BeTrue();
        os.Valor.Should().Be(355);
        os.Status.Should().Be(StatusOS.AguardandoAprovacao);
        repository.Verify(repo => repo.UpdateAsync(os), Times.Once);
    }

    [Fact]
    public async Task DeveBloquearEnvioQuandoOSNaoTiverItens()
    {
        var os = new OrdemServico(Guid.NewGuid(), string.Empty, string.Empty, StatusOS.EmDiagnostico, Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), DateTime.UtcNow, null, null, 0, 0, 0);
        var repository = new Mock<IOrdemServicoRepository>();
        repository.Setup(repo => repo.GetByIdAsync(os.Id)).ReturnsAsync(os);

        var estoqueRepository = new Mock<IEstoqueRepository>();

        var service = new EnviarOrcamentoService(repository.Object, estoqueRepository.Object, new EnviarOrcamentoCommandValidator());

        var act = () => service.EnviarOrcamento(new EnviarOrcamentoCommand { OrdemServicoId = os.Id });

        (await act.Should().ThrowAsync<InvalidOperationException>())
            .WithMessage("Informe ao menos um serviço ou produto antes de enviar o orçamento.");
        repository.Verify(repo => repo.UpdateAsync(It.IsAny<OrdemServico>()), Times.Never);
    }

    [Fact]
    public async Task DeveNotificarQuandoEstoqueForInsuficienteAntesDoEnvio()
    {
        var mecanicoId = Guid.NewGuid();
        var os = new OrdemServico(Guid.NewGuid(), string.Empty, string.Empty, StatusOS.EmDiagnostico, Guid.NewGuid(), mecanicoId, Guid.NewGuid(), DateTime.UtcNow, null, null, 0, 0, 0);

        var osp = new OrdemServicoProdutos(Guid.NewGuid(), os.Id, Guid.NewGuid(), 50, 3, 0, 0);
        osp.AdicionarProduto(Guid.NewGuid(), "Filtro", 0, Guid.NewGuid());
        os.AdicionarProdutos(osp);

        var repository = new Mock<IOrdemServicoRepository>();
        var estoqueRepository = new Mock<IEstoqueRepository>();
        var notificationService = new Mock<INotificationService>();
        repository.Setup(repo => repo.GetByIdAsync(os.Id)).ReturnsAsync(os);
        var service = new EnviarOrcamentoService(
            repository.Object,
            estoqueRepository.Object,
            new EnviarOrcamentoCommandValidator(),
            notificationService.Object);

        var act = () => service.EnviarOrcamento(new EnviarOrcamentoCommand { OrdemServicoId = os.Id });

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
}
