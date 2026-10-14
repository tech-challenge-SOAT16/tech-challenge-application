using TechChallenge.Domain.Entities;
using TechChallenge.Application.Abstractions.Repositories;

namespace TechChallenge.Application.Features.CategoriaProdutos.ListarCategoriasProdutos;

public class ListarCategoriasProdutosService
{
    private readonly ICategoriaProdutoRepository _categoriaProdutoRepository;

    public ListarCategoriasProdutosService(ICategoriaProdutoRepository categoriaProdutoRepository)
    {
        _categoriaProdutoRepository = categoriaProdutoRepository;
    }

    public async Task<List<CategoriaProduto>> ListarCategoriasProdutos(ListarCategoriasProdutosQuery query)
    {
        return await _categoriaProdutoRepository.GetAllAsync();
    }
}