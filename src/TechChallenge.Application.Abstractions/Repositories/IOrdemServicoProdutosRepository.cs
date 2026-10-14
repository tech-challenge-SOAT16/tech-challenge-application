using System;
using TechChallenge.Domain.Entities;

namespace TechChallenge.Application.Abstractions.Repositories
{
    public interface IOrdemServicoProdutosRepository : IRepository<OrdemServicoProdutos>
    {
        Task<bool> ExisteProdutoEmOrdemServicoAsync(Guid produtoId);
    }
}
