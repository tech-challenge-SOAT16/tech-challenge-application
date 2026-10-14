using FluentValidation;
using TechChallenge.Application.Abstractions.Repositories;

namespace TechChallenge.Application.Features.OS.ExcluirOS;

public class ExcluirOSService
{
    private readonly IOrdemServicoRepository _ordemServicoRepository;
    private readonly IValidator<ExcluirOSCommand> _validator;

    public ExcluirOSService(IOrdemServicoRepository ordemServicoRepository, IValidator<ExcluirOSCommand> validator)
    {
        _ordemServicoRepository = ordemServicoRepository;
        _validator = validator;
    }

    public async Task<bool> ExcluirOS(ExcluirOSCommand command)
    {
        _validator.ValidateAndThrow(command);

        var ordemServico = await _ordemServicoRepository.GetByIdAsync(command.Id);
        if (ordemServico is null)
            throw new KeyNotFoundException($"Ordem de serviço com Id {command.Id} não encontrada.");

        await _ordemServicoRepository.DeleteAsync(ordemServico);
        return true;
    }
}
