using Microsoft.EntityFrameworkCore;
using TechChallenge.Application.Abstractions.Repositories;
using TechChallenge.Domain.Entities;
using TechChallenge.Infrastructure.Database.Context;

namespace TechChallenge.Infrastructure.Database.Repositories
{
    public class CategoriaServicoRepository : Repository<CategoriaServico>, ICategoriaServicoRepository
    {
        #region Properties

        private readonly ApplicationDbContext _context;

        #endregion

        #region Constructor

        public CategoriaServicoRepository(ApplicationDbContext context) : base(context)
        {
            _context = context;
        }

        #endregion

        #region Members of ICategoriaServicoRepository

        public Task<CategoriaServico?> GetByDescricaoAsync(string descricao)
        {
            return _context.CategoriaServico.FirstOrDefaultAsync(c => c.Descricao == descricao);
        }

        #endregion
    }
}