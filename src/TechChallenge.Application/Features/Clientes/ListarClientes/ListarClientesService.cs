using TechChallenge.Domain.Entities;
using TechChallenge.Application.Abstractions.Repositories;

namespace TechChallenge.Application.Features.Clientes.ListarClientes;

public class ListarClientesService
{
    private readonly IClienteRepository _clienteRepository;

    public ListarClientesService(IClienteRepository clienteRepository)
    {
        _clienteRepository = clienteRepository;
    }

    public async Task<List<Cliente>> ListarClientes(ListarClientesQuery query)
    {
        return await _clienteRepository.GetAllAsync();
    }
}
