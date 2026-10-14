using TechChallenge.Domain.Entities;

namespace TechChallenge.Application.Abstractions.Repositories
{
    public interface ICategoriaServicoRepository : IRepository<CategoriaServico>
    {
        Task<CategoriaServico?> GetByDescricaoAsync(string descricao);
    }
}