using System;
using Microsoft.EntityFrameworkCore;
using TechChallenge.Domain.Entities;
using TechChallenge.Application.Abstractions.Repositories;
using TechChallenge.Infrastructure.Database.Context;

namespace TechChallenge.Infrastructure.Database.Repositories
{
    public class RefreshTokenRepository : Repository<RefreshToken>, IRefreshTokenRepository
    {
        private readonly ApplicationDbContext _context;

        public RefreshTokenRepository(ApplicationDbContext context) : base(context)
        {
            _context = context;
        }

        public async Task<RefreshToken?> GetByHashAsync(string hash)
        {
            return await _context.RefreshToken
                .Include(t => t.Usuario)
                .FirstOrDefaultAsync(t => t.TokenHash == hash);
        }

        public async Task<List<RefreshToken>> GetAtivasDoUsuarioAsync(Guid usuarioId, DateTime agora)
        {
            return await _context.RefreshToken
                .Where(t => t.UsuarioId == usuarioId
                            && t.RevogadoEm == null
                            && agora < t.ExpiraEm)
                .ToListAsync();
        }

        public async Task RevogarTodasDoUsuarioAsync(Guid usuarioId, DateTime agora)
        {
            var tokens = await _context.RefreshToken
                .Where(t => t.UsuarioId == usuarioId && t.RevogadoEm == null)
                .ToListAsync();

            foreach (var token in tokens)
            {
                token.AlterarRevogacao(agora);
            }

            await _context.SaveChangesAsync();
        }
    }
}
