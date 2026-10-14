using TechChallenge.Application.Abstractions.Repositories;

namespace TechChallenge.Application.Features.Estoque.ObterEstoque;

public class ObterEstoqueService
{
    private readonly IEstoqueRepository _estoqueRepository;

    public ObterEstoqueService(IEstoqueRepository estoqueRepository)
    {
        _estoqueRepository = estoqueRepository;
    }

    public async Task<Domain.Entities.Estoque> ObterEstoqueAsync(ObterEstoqueQuery query)
    {
        var estoque = await _estoqueRepository.GetByIdProdutoAsync(query.ProdutoId);

        if (estoque is null)
            throw new KeyNotFoundException($"Estoque do produto {query.ProdutoId} não encontrado.");

        return estoque;
    }
}
