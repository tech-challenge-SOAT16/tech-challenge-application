using TechChallenge.Domain.Entities;
using TechChallenge.Application.Abstractions.Repositories;

namespace TechChallenge.Application.Features.Clientes.ObterCliente;

public class ObterClienteService
{
    private readonly IClienteRepository _clienteRepository;

    public ObterClienteService(IClienteRepository clienteRepository)
    {
        _clienteRepository = clienteRepository;
    }

    public async Task<Cliente> ObterCliente(ObterClienteQuery query)
    {
        var cliente = await _clienteRepository.GetByIdAsync(query.Id);
        if (cliente is null)
            throw new KeyNotFoundException($"Cliente com Id {query.Id} não encontrado.");

        return cliente;
    }
}
