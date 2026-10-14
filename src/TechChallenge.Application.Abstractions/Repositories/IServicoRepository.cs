using TechChallenge.Domain.Entities;

namespace TechChallenge.Application.Abstractions.Repositories
{
    public interface IServicoRepository : IRepository<Servico>
    {
        Task<bool> ExisteServicoComCategoria(Guid categoriaId);
    }
}