using FluentValidation;
using TechChallenge.Application.Abstractions.Notifications;
using TechChallenge.Application.Abstractions.Repositories;
using TechChallenge.Application.Notifications;
using TechChallenge.Domain.Enums;

namespace TechChallenge.Application.Features.OS.ReprovarOrcamento;

public class ReprovarOrcamentoService
{
    private readonly IOrdemServicoRepository _ordemServicoRepository;
    private readonly IValidator<ReprovarOrcamentoCommand> _validator;
    private readonly INotificationService _notificationService;

    public ReprovarOrcamentoService(
        IOrdemServicoRepository ordemServicoRepository,
        IValidator<ReprovarOrcamentoCommand> validator,
        INotificationService? notificationService = null)
    {
        _ordemServicoRepository = ordemServicoRepository;
        _validator = validator;
        _notificationService = notificationService ?? NullNotificationService.Instance;
    }

    public async Task<bool> ReprovarOrcamento(ReprovarOrcamentoCommand command)
    {
        _validator.ValidateAndThrow(command);

        var os = await _ordemServicoRepository.GetByIdAsync(command.OrdemServicoId);
        if (os is null)
            throw new KeyNotFoundException($"OS com Id {command.OrdemServicoId} não encontrada.");

        var statusAnterior = os.Status;

        os.TransicionarPara(StatusOS.Reprovada);
        _notificationService.NotificarTransicaoOS(os, statusAnterior);
        await _ordemServicoRepository.UpdateAsync(os);
        return true;
    }
}
