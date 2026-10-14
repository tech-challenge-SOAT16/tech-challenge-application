using FluentValidation;
using TechChallenge.Application.Abstractions.Repositories;

namespace TechChallenge.Application.Features.Veiculos.ExcluirVeiculo;

public class ExcluirVeiculoService
{
    private readonly IVeiculoRepository _veiculoRepository;
    private readonly IOrdemServicoRepository _ordemServicoRepository;
    private readonly IValidator<ExcluirVeiculoCommand> _validator;

    public ExcluirVeiculoService(
        IVeiculoRepository veiculoRepository,
        IOrdemServicoRepository ordemServicoRepository,
        IValidator<ExcluirVeiculoCommand> validator)
    {
        _veiculoRepository = veiculoRepository;
        _ordemServicoRepository = ordemServicoRepository;
        _validator = validator;
    }

    public async Task<bool> ExcluirVeiculo(ExcluirVeiculoCommand command)
    {
        _validator.ValidateAndThrow(command);

        var veiculo = await _veiculoRepository.GetByIdAsync(command.Id);
        if (veiculo is null)
            throw new KeyNotFoundException($"Veículo com Id {command.Id} não encontrado.");

        var veiculoPossuiOrdemServico = await _ordemServicoRepository.ExistePorVeiculoAsync(command.Id);
        if (veiculoPossuiOrdemServico)
            throw new InvalidOperationException("Não é possível excluir um veículo associado a uma ordem de serviço.");

        await _veiculoRepository.DeleteAsync(veiculo);
        return true;
    }
}
