using TechChallenge.Domain.Entities;
using TechChallenge.Application.Abstractions.Repositories;

namespace TechChallenge.Application.Features.CategoriaProdutos.ObterCategoriaProduto;

public class ObterCategoriaProdutoService
{
    private readonly ICategoriaProdutoRepository _categoriaProdutoRepository;

    public ObterCategoriaProdutoService(ICategoriaProdutoRepository categoriaProdutoRepository)
    {
        _categoriaProdutoRepository = categoriaProdutoRepository;
    }

    public async Task<CategoriaProduto> ObterCategoriaProduto(ObterCategoriaProdutoQuery query)
    {
        var categoriaProduto = await _categoriaProdutoRepository.GetByIdAsync(query.Id);

        if (categoriaProduto is null)
            throw new KeyNotFoundException($"Categoria de produto com Id {query.Id} não encontrada.");

        return categoriaProduto;
    }
}