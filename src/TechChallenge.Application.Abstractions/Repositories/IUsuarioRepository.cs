using System;
using TechChallenge.Domain.Entities;

namespace TechChallenge.Application.Abstractions.Repositories
{
    public interface IUsuarioRepository : IRepository<Usuario>
    {
        Task<Usuario?> GetByLoginAsync(string login);
        Task<bool> ExisteLoginAsync(string login);
        Task<bool> ExisteVinculoFuncionarioAsync(Guid funcionarioId);
    }
}
