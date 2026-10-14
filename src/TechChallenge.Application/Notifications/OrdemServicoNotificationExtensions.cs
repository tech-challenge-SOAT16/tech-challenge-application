using TechChallenge.Application.Abstractions.Notifications;
using TechChallenge.Domain.Entities;
using TechChallenge.Domain.Enums;

namespace TechChallenge.Application.Notifications;

public static class OrdemServicoNotificationExtensions
{
    public static void NotificarTransicaoOS(this INotificationService notificationService, OrdemServico ordemServico, StatusOS statusAnterior)
    {
        var mensagem = $"OS {ordemServico.Id} mudou de {statusAnterior} para {ordemServico.Status}.";

        if (ordemServico.FuncionarioResponsavelId == Guid.Empty)
        {
            notificationService.NotificarFuncionariosPorFuncao(
                TipoFuncionario.Administrador,
                "Status da OS atualizado",
                mensagem);

            return;
        }

        notificationService.NotificarFuncionario(
            ordemServico.FuncionarioResponsavelId,
            "Status da OS atualizado",
            mensagem);
    }
}
