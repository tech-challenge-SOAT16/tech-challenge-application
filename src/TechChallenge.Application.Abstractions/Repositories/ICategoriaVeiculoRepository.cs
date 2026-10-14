using TechChallenge.Domain.Entities;

namespace TechChallenge.Application.Abstractions.Repositories
{
    public interface ICategoriaVeiculoRepository : IRepository<CategoriaVeiculo>
    {
        Task<CategoriaVeiculo?> GetByDescricaoAsync(string descricao);
    }
}