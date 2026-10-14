using TechChallenge.Domain.Entities;
using TechChallenge.Application.Abstractions.Repositories;

namespace TechChallenge.Application.Features.Estoque.ListarEstoques;

public class ListarEstoquesService
{
    private readonly IEstoqueRepository _estoqueRepository;

    public ListarEstoquesService(IEstoqueRepository estoqueRepository)
    {
        _estoqueRepository = estoqueRepository;
    }

    public Task<List<Domain.Entities.Estoque>> ListarEstoquesAsync(ListarEstoquesQuery query)
    {
        return _estoqueRepository.GetAllAsync();
    }
}
