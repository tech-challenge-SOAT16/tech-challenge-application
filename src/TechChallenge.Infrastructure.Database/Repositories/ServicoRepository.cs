using Microsoft.EntityFrameworkCore;
using TechChallenge.Domain.Entities;
using TechChallenge.Application.Abstractions.Repositories;
using TechChallenge.Infrastructure.Database.Context;

namespace TechChallenge.Infrastructure.Database.Repositories
{
    public class ServicoRepository : Repository<Servico>, IServicoRepository
    {
        #region Properties

        private readonly ApplicationDbContext _context;

        #endregion

        #region Constructor

        public ServicoRepository(ApplicationDbContext context) : base(context)
        {
            _context = context;
        }

        #endregion

        #region Member of IServicoRepository

        public async Task<bool> ExisteServicoComCategoria(Guid categoriaId)
        {
            return await _context.Servico
                .AnyAsync(s => s.CategoriaId == categoriaId);
        }

        #endregion
    }
}