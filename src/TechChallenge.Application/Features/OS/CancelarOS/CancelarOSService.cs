using FluentValidation;
using TechChallenge.Application.Abstractions.Notifications;
using TechChallenge.Application.Abstractions.Repositories;
using TechChallenge.Application.Notifications;
using TechChallenge.Domain.Enums;

namespace TechChallenge.Application.Features.OS.CancelarOS;

public class CancelarOSService
{
    private readonly IOrdemServicoRepository _ordemServicoRepository;
    private readonly IValidator<CancelarOSCommand> _validator;
    private readonly INotificationService _notificationService;

    public CancelarOSService(
        IOrdemServicoRepository ordemServicoRepository,
        IValidator<CancelarOSCommand> validator,
        INotificationService? notificationService = null)
    {
        _ordemServicoRepository = ordemServicoRepository;
        _validator = validator;
        _notificationService = notificationService ?? NullNotificationService.Instance;
    }

    public async Task<bool> CancelarOS(CancelarOSCommand command)
    {
        _validator.ValidateAndThrow(command);

        var os = await _ordemServicoRepository.GetByIdAsync(command.OrdemServicoId);
        if (os is null)
            throw new KeyNotFoundException($"OS com Id {command.OrdemServicoId} não encontrada.");

        var statusAnterior = os.Status;

        os.TransicionarPara(StatusOS.Cancelada);
        _notificationService.NotificarTransicaoOS(os, statusAnterior);
        await _ordemServicoRepository.UpdateAsync(os);
        return true;
    }
}
