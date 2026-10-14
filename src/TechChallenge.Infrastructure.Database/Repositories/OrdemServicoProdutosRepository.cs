using System;
using Microsoft.EntityFrameworkCore;
using TechChallenge.Application.Abstractions.Repositories;
using TechChallenge.Domain.Entities;
using TechChallenge.Infrastructure.Database.Context;

namespace TechChallenge.Infrastructure.Database.Repositories
{
    public class OrdemServicoProdutosRepository : Repository<OrdemServicoProdutos>, IOrdemServicoProdutosRepository
    {
        #region Properties

        private readonly ApplicationDbContext _context;

        #endregion

        #region Constructor

        public OrdemServicoProdutosRepository(ApplicationDbContext context) : base(context)
        {
            _context = context;
        }

        #endregion

        #region Members of IOrdemServicoProdutosRepository

        public override async Task<OrdemServicoProdutos?> GetByIdAsync(Guid id)
        {
            return await _context.OrdemServicoProdutos.FirstOrDefaultAsync(os => os.Id == id);
        }

        public override async Task<List<OrdemServicoProdutos>> GetAllAsync()
        {
            return await _context.OrdemServicoProdutos.ToListAsync();
        }

        public async Task<bool> ExisteProdutoEmOrdemServicoAsync(Guid produtoId)
        {
            return await _context.OrdemServicoProdutos.AnyAsync(item => item.ProdutoId == produtoId);
        }

        #endregion
    }
}
