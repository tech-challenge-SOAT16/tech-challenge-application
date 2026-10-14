using TechChallenge.Application.Abstractions.Repositories;
using TechChallenge.Domain.Entities;

namespace TechChallenge.Application.Features.Inventario.ObterItemInventario;

public class ObterItemInventarioService
{
    private readonly IProdutoRepository _produtoRepository;

    public ObterItemInventarioService(IProdutoRepository produtoRepository)
    {
        _produtoRepository = produtoRepository;
    }

    public async Task<Produto> ObterItemInventario(ObterItemInventarioQuery query)
    {
        var produto = await _produtoRepository.GetByIdAsync(query.Id);
        if (produto is null)
            throw new KeyNotFoundException($"Item de inventário com Id {query.Id} não encontrado.");

        return produto;
    }
}
