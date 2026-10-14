using TechChallenge.Application.Abstractions.Notifications;
using TechChallenge.Domain.Enums;

namespace TechChallenge.Application.Notifications;

public class NullNotificationService : INotificationService
{
    public static NullNotificationService Instance { get; } = new();

    private NullNotificationService()
    {
    }

    public void NotificarFuncionario(Guid funcionarioId, string titulo, string mensagem)
    {
    }

    public void NotificarFuncionariosPorFuncao(TipoFuncionario tipoFuncionario, string titulo, string mensagem)
    {
    }
}
