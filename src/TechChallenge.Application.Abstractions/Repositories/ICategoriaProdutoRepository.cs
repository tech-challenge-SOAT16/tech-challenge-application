using TechChallenge.Domain.Entities;

namespace TechChallenge.Application.Abstractions.Repositories
{
    public interface ICategoriaProdutoRepository : IRepository<CategoriaProduto>
    {
        Task<CategoriaProduto?> GetByDescricaoAsync(string descricao);
    }
}