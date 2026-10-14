using TechChallenge.Domain.Entities;

namespace TechChallenge.Application.Abstractions.Repositories
{
    public interface IProdutoRepository : IRepository<Produto>
    {
        Task<bool> ExisteProdutoComCategoria(Guid categoriaId);
    }
}