using System;
using TechChallenge.Domain.Entities;

namespace TechChallenge.Application.Abstractions.Repositories
{
    public interface IOrdemServicoServicosRepository : IRepository<OrdemServicoServicos>
    {
        Task<bool> ExisteServicoEmOrdemServicoAsync(Guid servicoId);
    }
}
