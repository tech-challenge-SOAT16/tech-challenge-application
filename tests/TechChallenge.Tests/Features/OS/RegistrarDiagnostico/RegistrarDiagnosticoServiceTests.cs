using FluentAssertions;
using Moq;
using TechChallenge.Application.Abstractions.Notifications;
using TechChallenge.Application.Abstractions.Repositories;
using TechChallenge.Application.Features.OS.RegistrarDiagnostico;
using TechChallenge.Domain.Entities;
using TechChallenge.Domain.Enums;

namespace TechChallenge.Tests.Features.OS.RegistrarDiagnostico;

public class RegistrarDiagnosticoServiceTests
{
    [Fact]
    public async Task DeveNotificarQuandoProdutoNaoTiverEstoque()
    {
        var osId = Guid.NewGuid();
        var produtoId = Guid.NewGuid();
        var mecanicoId = Guid.NewGuid();
        var os = new OrdemServico(osId, string.Empty, string.Empty, StatusOS.EmDiagnostico, Guid.NewGuid(), mecanicoId, Guid.NewGuid(), DateTime.UtcNow, null, null, 0, 0, 0);
        var ordemServicoRepository = new Mock<IOrdemServicoRepository>();
        var servicoRepository = new Mock<IServicoRepository>();
        var produtoRepository = new Mock<IProdutoRepository>();
        var estoqueRepository = new Mock<IEstoqueRepository>();
        var notificationService = new Mock<INotificationService>();
        ordemServicoRepository.Setup(repo => repo.GetByIdAsync(osId)).ReturnsAsync(os);
        produtoRepository.Setup(repo => repo.GetByIdAsync(produtoId))
            .ReturnsAsync(new Produto(produtoId, "Filtro", 0, Guid.NewGuid()));
        var service = new RegistrarDiagnosticoService(
            ordemServicoRepository.Object,
            servicoRepository.Object,
            produtoRepository.Object,
            estoqueRepository.Object,
            new RegistrarDiagnosticoCommandValidator(),
            notificationService.Object);

        var act = () => service.RegistrarDiagnostico(new RegistrarDiagnosticoCommand
        {
            OrdemServicoId = osId,
            Produtos = [new ItemDiagnosticoCommand { Id = produtoId, Quantidade = 2 }]
        });

        (await act.Should().ThrowAsync<InvalidOperationException>())
            .WithMessage("Estoque insuficiente para o produto Filtro.");
        notificationService.Verify(service => service.NotificarFuncionariosPorFuncao(
            TipoFuncionario.Administrador,
            "Estoque insuficiente",
            It.Is<string>(mensagem => mensagem.Contains(osId.ToString()) && mensagem.Contains("Filtro"))), Times.Once);
        notificationService.Verify(service => service.NotificarFuncionario(
            mecanicoId,
            "Estoque insuficiente",
            It.Is<string>(mensagem => mensagem.Contains(osId.ToString()) && mensagem.Contains("Filtro"))), Times.Once);
        ordemServicoRepository.Verify(repo => repo.UpdateAsync(It.IsAny<OrdemServico>()), Times.Never);
    }
}
