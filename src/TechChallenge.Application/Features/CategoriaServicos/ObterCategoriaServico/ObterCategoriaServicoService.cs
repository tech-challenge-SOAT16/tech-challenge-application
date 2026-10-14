using TechChallenge.Domain.Entities;
using TechChallenge.Application.Abstractions.Repositories;

namespace TechChallenge.Application.Features.CategoriaServicos.ObterCategoriaServico;

public class ObterCategoriaServicoService
{
    private readonly ICategoriaServicoRepository _categoriaServicoRepository;

    public ObterCategoriaServicoService(ICategoriaServicoRepository categoriaServicoRepository)
    {
        _categoriaServicoRepository = categoriaServicoRepository;
    }

    public async Task<CategoriaServico> ObterCategoriaServico(ObterCategoriaServicoQuery query)
    {
        var categoriaServico = await _categoriaServicoRepository.GetByIdAsync(query.Id);

        if (categoriaServico is null)
            throw new KeyNotFoundException($"Categoria de Serviço com Id {query.Id} não encontrada.");

        return categoriaServico;
    }
}