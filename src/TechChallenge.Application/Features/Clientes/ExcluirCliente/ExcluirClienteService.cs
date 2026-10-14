using FluentValidation;
using TechChallenge.Application.Abstractions.Repositories;

namespace TechChallenge.Application.Features.Clientes.ExcluirCliente;

public class ExcluirClienteService
{
    private readonly IClienteRepository _clienteRepository;
    private readonly IOrdemServicoRepository _ordemServicoRepository;
    private readonly IValidator<ExcluirClienteCommand> _validator;

    public ExcluirClienteService(
        IClienteRepository clienteRepository,
        IOrdemServicoRepository ordemServicoRepository,
        IValidator<ExcluirClienteCommand> validator)
    {
        _clienteRepository = clienteRepository;
        _ordemServicoRepository = ordemServicoRepository;
        _validator = validator;
    }

    public async Task<bool> ExcluirCliente(ExcluirClienteCommand command)
    {
        _validator.ValidateAndThrow(command);

        var cliente = await _clienteRepository.GetByIdAsync(command.Id);
        if (cliente is null)
            throw new KeyNotFoundException($"Cliente com Id {command.Id} não encontrado.");

        var clientePossuiOrdemServico = await _ordemServicoRepository.ExistePorClienteAsync(command.Id);
        if (clientePossuiOrdemServico)
            throw new InvalidOperationException("Não é possível excluir um cliente associado a uma ordem de serviço.");

        await _clienteRepository.DeleteAsync(cliente);
        return true;
    }
}
