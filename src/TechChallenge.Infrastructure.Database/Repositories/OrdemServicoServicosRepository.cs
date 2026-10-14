using System;
using Microsoft.EntityFrameworkCore;
using TechChallenge.Application.Abstractions.Repositories;
using TechChallenge.Domain.Entities;
using TechChallenge.Infrastructure.Database.Context;

namespace TechChallenge.Infrastructure.Database.Repositories
{
    public class OrdemServicoServicosRepository : Repository<OrdemServicoServicos>, IOrdemServicoServicosRepository
    {
        #region Properties

        private readonly ApplicationDbContext _context;

        #endregion

        #region Constructor

        public OrdemServicoServicosRepository(ApplicationDbContext context) : base(context)
        {
            _context = context;
        }

        #endregion

        #region Members of IOrdemServicoServicosRepository

        public override async Task<OrdemServicoServicos?> GetByIdAsync(Guid id)
        {
            return await _context.OrdemServicoServicos.FirstOrDefaultAsync(os => os.Id == id);
        }

        public override async Task<List<OrdemServicoServicos>> GetAllAsync()
        {
            return await _context.OrdemServicoServicos.ToListAsync();
        }

        public async Task<bool> ExisteServicoEmOrdemServicoAsync(Guid servicoId)
        {
            return await _context.OrdemServicoServicos.AnyAsync(item => item.ServicoId == servicoId);
        }

        #endregion
    }
}
