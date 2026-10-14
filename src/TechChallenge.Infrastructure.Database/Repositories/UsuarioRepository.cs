using System;
using Microsoft.EntityFrameworkCore;
using TechChallenge.Domain.Entities;
using TechChallenge.Application.Abstractions.Repositories;
using TechChallenge.Infrastructure.Database.Context;

namespace TechChallenge.Infrastructure.Database.Repositories
{
    public class UsuarioRepository : Repository<Usuario>, IUsuarioRepository
    {
        private readonly ApplicationDbContext _context;

        public UsuarioRepository(ApplicationDbContext context) : base(context)
        {
            _context = context;
        }

        public async Task<Usuario?> GetByLoginAsync(string login)
        {
            var loginNormalizado = login.ToUpper();
            return await _context.Usuario
                .FirstOrDefaultAsync(u => u.Login.ToUpper() == loginNormalizado);
        }

        public async Task<bool> ExisteLoginAsync(string login)
        {
            var loginNormalizado = login.ToUpper();
            return await _context.Usuario.AnyAsync(u => u.Login.ToUpper() == loginNormalizado);
        }

        public async Task<bool> ExisteVinculoFuncionarioAsync(Guid funcionarioId)
        {
            return await _context.Usuario.AnyAsync(u => u.FuncionarioId == funcionarioId);
        }
    }
}
