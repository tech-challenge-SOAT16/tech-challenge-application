using System;
using TechChallenge.Application.Abstractions.Notifications;
using FluentValidation;
using TechChallenge.Domain.Enums;
using TechChallenge.Application.Notifications;
using TechChallenge.Application.Abstractions.Repositories;

namespace TechChallenge.Application.Features.OS.AtribuirOS;

public class AtribuirOSService
{
    private readonly IOrdemServicoRepository _ordemServicoRepository;
    private readonly IFuncionarioRepository _funcionarioRepository;
    private readonly IValidator<AtribuirOSCommand> _validator;
    private readonly INotificationService _notificationService;

    public AtribuirOSService(
        IOrdemServicoRepository ordemServicoRepository,
        IFuncionarioRepository funcionarioRepository,
        IValidator<AtribuirOSCommand> validator,
        INotificationService? notificationService = null)
    {
        _ordemServicoRepository = ordemServicoRepository;
        _funcionarioRepository = funcionarioRepository;
        _validator = validator;
        _notificationService = notificationService ?? NullNotificationService.Instance;
    }

    public async Task<bool> AtribuirOS(AtribuirOSCommand command)
    {
        _validator.ValidateAndThrow(command);

        var mecanico = await _funcionarioRepository.GetByIdAsync(command.MecanicoId);
        if (mecanico is null)
            throw new KeyNotFoundException($"Mecânico com Id {command.MecanicoId} não encontrado.");

        // RF10: mecânico só pode ter 1 OS ativa por vez
        var osAtiva = await _ordemServicoRepository.GetOSAtivaMecanicoAsync(command.MecanicoId);
        if (osAtiva is not null)
            throw new InvalidOperationException($"Mecânico já possui uma OS ativa (Id: {osAtiva.Id}).");

        var os = await _ordemServicoRepository.GetByIdAsync(command.OrdemServicoId);
        if (os is null)
            throw new KeyNotFoundException($"OS com Id {command.OrdemServicoId} não encontrada.");

        var statusAnterior = os.Status;

        os.AtribuirFuncionario(command.MecanicoId);
        os.TransicionarPara(StatusOS.EmDiagnostico);
        _notificationService.NotificarTransicaoOS(os, statusAnterior);

        await _ordemServicoRepository.UpdateAsync(os);
        return true;
    }
}
