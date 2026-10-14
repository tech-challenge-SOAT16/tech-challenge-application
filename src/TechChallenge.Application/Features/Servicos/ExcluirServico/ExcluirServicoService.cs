using FluentValidation;
using TechChallenge.Application.Abstractions.Repositories;

namespace TechChallenge.Application.Features.Servicos.ExcluirServico;

public class ExcluirServicoService
{
    private readonly IServicoRepository _servicoRepository;
    private readonly IOrdemServicoServicosRepository _ordemServicoServicosRepository;
    private readonly IValidator<ExcluirServicoCommand> _validator;

    public ExcluirServicoService(
        IServicoRepository servicoRepository,
        IOrdemServicoServicosRepository ordemServicoServicosRepository,
        IValidator<ExcluirServicoCommand> validator)
    {
        _servicoRepository = servicoRepository;
        _ordemServicoServicosRepository = ordemServicoServicosRepository;
        _validator = validator;
    }

    public async Task<bool> ExcluirServico(ExcluirServicoCommand command)
    {
        _validator.ValidateAndThrow(command);

        var servico = await _servicoRepository.GetByIdAsync(command.Id);
        if (servico is null)
            throw new KeyNotFoundException($"Serviço com Id {command.Id} não encontrado.");

        var servicoEmUso = await _ordemServicoServicosRepository.ExisteServicoEmOrdemServicoAsync(command.Id);
        if (servicoEmUso)
            throw new InvalidOperationException("Não é possível excluir um serviço associado a uma ordem de serviço.");

        await _servicoRepository.DeleteAsync(servico);
        return true;
    }
}
