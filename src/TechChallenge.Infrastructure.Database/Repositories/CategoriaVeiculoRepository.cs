using Microsoft.EntityFrameworkCore;
using TechChallenge.Application.Abstractions.Repositories;
using TechChallenge.Domain.Entities;
using TechChallenge.Infrastructure.Database.Context;

namespace TechChallenge.Infrastructure.Database.Repositories
{
    public class CategoriaVeiculoRepository : Repository<CategoriaVeiculo>, ICategoriaVeiculoRepository
    {
        #region Properties

        private readonly ApplicationDbContext _context;

        #endregion

        #region Constructor

        public CategoriaVeiculoRepository(ApplicationDbContext context) : base(context)
        {
            _context = context;
        }

        #endregion

        #region Members of ICategoriaVeiculoRepository

        public Task<CategoriaVeiculo?> GetByDescricaoAsync(string descricao)
        {
            return _context.CategoriaVeiculo.FirstOrDefaultAsync(c => c.Descricao == descricao);
        }

        #endregion
    }
}