using Moq;
using TechChallenge.Application.Abstractions.Notifications;
using TechChallenge.Application.Notifications;
using TechChallenge.Domain.Entities;
using TechChallenge.Domain.Enums;

namespace TechChallenge.Tests.Notifications;

public class OrdemServicoNotificationExtensionsTests
{
    [Fact]
    public void DeveNotificarFuncionarioResponsavelQuandoOSTransicionar()
    {
        var funcionarioId = Guid.NewGuid();
        var os = new OrdemServico(Guid.NewGuid(), "Descrição da OS", string.Empty, StatusOS.EmExecucao, Guid.NewGuid(), funcionarioId, Guid.NewGuid(), DateTime.Now, null, null, 0, 0, 0);
        var notificationService = new Mock<INotificationService>();

        notificationService.Object.NotificarTransicaoOS(os, StatusOS.AguardandoAprovacao);

        notificationService.Verify(service => service.NotificarFuncionario(
            funcionarioId,
            "Status da OS atualizado",
            It.Is<string>(mensagem =>
                mensagem.Contains(os.Id.ToString()) &&
                mensagem.Contains(StatusOS.AguardandoAprovacao.ToString()) &&
                mensagem.Contains(StatusOS.EmExecucao.ToString()))), Times.Once);
        notificationService.Verify(service => service.NotificarFuncionariosPorFuncao(
            It.IsAny<TipoFuncionario>(),
            It.IsAny<string>(),
            It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public void DeveNotificarAdministradorQuandoOSNaoTiverFuncionarioResponsavel()
    {
        var os = new OrdemServico(Guid.NewGuid(), "Descrição da OS", string.Empty, StatusOS.Cancelada, Guid.NewGuid(), Guid.Empty, Guid.NewGuid(), DateTime.Now, null, null, 0, 0, 0);
        var notificationService = new Mock<INotificationService>();

        notificationService.Object.NotificarTransicaoOS(os, StatusOS.Recebida);

        notificationService.Verify(service => service.NotificarFuncionariosPorFuncao(
            TipoFuncionario.Administrador,
            "Status da OS atualizado",
            It.Is<string>(mensagem =>
                mensagem.Contains(os.Id.ToString()) &&
                mensagem.Contains(StatusOS.Recebida.ToString()) &&
                mensagem.Contains(StatusOS.Cancelada.ToString()))), Times.Once);
        notificationService.Verify(service => service.NotificarFuncionario(
            It.IsAny<Guid>(),
            It.IsAny<string>(),
            It.IsAny<string>()), Times.Never);
    }
}
