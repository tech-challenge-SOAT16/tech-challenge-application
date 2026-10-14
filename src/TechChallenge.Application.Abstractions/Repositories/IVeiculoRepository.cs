using TechChallenge.Domain.Entities;

namespace TechChallenge.Application.Abstractions.Repositories
{
    public interface IVeiculoRepository : IRepository<Veiculo>
    {
        Task<Veiculo?> GetByPlacaAsync(string placa);
        Task<bool> ExisteVeiculoComCategoria(Guid categoriaId);
    }
}