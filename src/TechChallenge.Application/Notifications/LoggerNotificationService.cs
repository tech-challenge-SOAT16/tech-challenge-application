using Microsoft.Extensions.Logging;
using TechChallenge.Application.Abstractions.Notifications;
using TechChallenge.Domain.Enums;

namespace TechChallenge.Application.Notifications;

public class LoggerNotificationService : INotificationService
{
    private readonly ILogger<LoggerNotificationService> _logger;

    public LoggerNotificationService(ILogger<LoggerNotificationService> logger)
    {
        _logger = logger;
    }

    public void NotificarFuncionario(Guid funcionarioId, string titulo, string mensagem)
    {
        _logger.LogInformation(
            "Notificação para funcionário {FuncionarioId}: {Titulo} - {Mensagem}",
            funcionarioId,
            titulo,
            mensagem);
    }

    public void NotificarFuncionariosPorFuncao(TipoFuncionario tipoFuncionario, string titulo, string mensagem)
    {
        _logger.LogInformation(
            "Notificação para funcionários com função {TipoFuncionario}: {Titulo} - {Mensagem}",
            tipoFuncionario,
            titulo,
            mensagem);
    }
}
