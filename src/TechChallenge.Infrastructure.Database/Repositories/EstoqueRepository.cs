using Microsoft.EntityFrameworkCore;
using TechChallenge.Domain.Entities;
using TechChallenge.Application.Abstractions.Repositories;
using TechChallenge.Infrastructure.Database.Context;

namespace TechChallenge.Infrastructure.Database.Repositories
{
    public class EstoqueRepository : Repository<Estoque>, IEstoqueRepository
    {
        #region Properties

        private readonly ApplicationDbContext _context;

        #endregion

        #region Constructor

        public EstoqueRepository(ApplicationDbContext context) : base(context)
        {
            _context = context;
        }

        #endregion

        #region Members of IEstoqueRepository

        public async Task<Estoque?> GetByIdProdutoAsync(Guid idProduto)
        {
            return await _context.Estoque
                .FirstOrDefaultAsync(e => e.ProdutoId == idProduto);
        }

        public override async Task<List<Estoque>> GetAllAsync()
        {
            return await _context.Estoque
                .Include(e => e.Produto)
                .ToListAsync();
        }

        #endregion
    }
}