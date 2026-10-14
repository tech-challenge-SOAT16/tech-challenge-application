using FluentAssertions;
using Moq;
using TechChallenge.Application.Abstractions.Notifications;
using TechChallenge.Application.Abstractions.Repositories;
using TechChallenge.Application.Features.OS.CriarOS;
using TechChallenge.Domain.Entities;
using TechChallenge.Domain.Enums;

namespace TechChallenge.Tests.Features.OS.CriarOS;

public class CriarOSServiceTests
{
    [Fact]
    public async Task DeveCriarOSEnotificarMecanicos()
    {
        var clienteId = Guid.NewGuid();
        var funcionarioId = Guid.NewGuid();
        var veiculoId = Guid.NewGuid();
        var ordemServicoRepository = new Mock<IOrdemServicoRepository>();
        var clienteRepository = new Mock<IClienteRepository>();
        var funcionarioRepository = new Mock<IFuncionarioRepository>();
        var veiculoRepository = new Mock<IVeiculoRepository>();
        var servicoRepository = new Mock<IServicoRepository>();
        var produtoRepository = new Mock<IProdutoRepository>();
        var estoqueRepository = new Mock<IEstoqueRepository>();
        var notificationService = new Mock<INotificationService>();
        clienteRepository.Setup(repo => repo.GetByIdAsync(clienteId))
            .ReturnsAsync(new Cliente(clienteId, string.Empty, "cliente@teste.local", TipoDocumento.Cpf, string.Empty, Guid.NewGuid()));
        funcionarioRepository.Setup(repo => repo.GetByIdAsync(funcionarioId))
            .ReturnsAsync(new Funcionario(funcionarioId, string.Empty, string.Empty, string.Empty, TipoFuncionario.Mecanico, Guid.NewGuid()));
        veiculoRepository.Setup(repo => repo.GetByIdAsync(veiculoId))
            .ReturnsAsync(new Veiculo(veiculoId, string.Empty, string.Empty, string.Empty, string.Empty, 1999, 0, 0, Guid.NewGuid(), Guid.NewGuid()));
        ordemServicoRepository.Setup(repo => repo.AddAsync(It.IsAny<OrdemServico>()))
            .Returns<OrdemServico>(os => Task.FromResult(os));
        var service = new CriarOSService(
            ordemServicoRepository.Object,
            clienteRepository.Object,
            funcionarioRepository.Object,
            veiculoRepository.Object,
            servicoRepository.Object,
            produtoRepository.Object,
            estoqueRepository.Object,
            new CriarOSCommandValidator(),
            notificationService.Object);

        var osId = await service.CriarOSAsync(new CriarOSCommand
        {
            Descricao = "Troca de óleo",
            ClienteResponsavelId = clienteId,
            FuncionarioResponsavelId = funcionarioId,
            VeiculoId = veiculoId
        });

        osId.Should().NotBeEmpty();
        notificationService.Verify(service => service.NotificarFuncionariosPorFuncao(
            TipoFuncionario.Mecanico,
            "Nova OS na fila",
            It.Is<string>(mensagem => mensagem.Contains(osId.ToString()))), Times.Once);
    }
}
