using Microsoft.EntityFrameworkCore;
using TechChallenge.Application.Abstractions.Repositories;
using TechChallenge.Domain.Entities;
using TechChallenge.Infrastructure.Database.Context;

namespace TechChallenge.Infrastructure.Database.Repositories
{
    public class CategoriaProdutoRepository : Repository<CategoriaProduto>, ICategoriaProdutoRepository
    {
        #region Properties

        private readonly ApplicationDbContext _context;

        #endregion

        #region Constructor

        public CategoriaProdutoRepository(ApplicationDbContext context) : base(context)
        {
            _context = context;
        }

        #endregion

        #region Members of ICategoriaProdutoRepository

        public Task<CategoriaProduto?> GetByDescricaoAsync(string descricao)
        {
            return _context.CategoriaProduto.FirstOrDefaultAsync(c => c.Descricao == descricao);
        }

        #endregion
    }
}