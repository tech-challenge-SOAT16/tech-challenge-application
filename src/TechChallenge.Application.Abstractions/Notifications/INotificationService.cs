using TechChallenge.Domain.Enums;

namespace TechChallenge.Application.Abstractions.Notifications;

public interface INotificationService
{
    void NotificarFuncionario(Guid funcionarioId, string titulo, string mensagem);
    void NotificarFuncionariosPorFuncao(TipoFuncionario tipoFuncionario, string titulo, string mensagem);
}
