using TechChallenge.Domain.Entities;

namespace TechChallenge.Application.Abstractions.Repositories
{
    public interface IEstoqueRepository : IRepository<Estoque>
    {
        Task<Estoque?> GetByIdProdutoAsync(Guid idProduto);
    }
}