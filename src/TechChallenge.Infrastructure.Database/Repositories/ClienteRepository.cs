using System;
using Microsoft.EntityFrameworkCore;
using TechChallenge.Domain.Entities;
using TechChallenge.Application.Abstractions.Repositories;
using TechChallenge.Infrastructure.Database.Context;

namespace TechChallenge.Infrastructure.Database.Repositories
{
    public class ClienteRepository : Repository<Cliente>, IClienteRepository
    {
        #region Properties

        private readonly ApplicationDbContext _context;

        #endregion

        #region Constructor

        public ClienteRepository(ApplicationDbContext context) : base(context)
        {
            _context = context;
        }

        #endregion

        #region Members of IClienteRepository

        public override async Task<Cliente?> GetByIdAsync(Guid id)
        {
            return await _context.Cliente
                .Include(c => c.Endereco)
                .FirstOrDefaultAsync(c => c.Id == id);
        }

        public override async Task<List<Cliente>> GetAllAsync()
        {
            return await _context.Cliente
                .Include(c => c.Endereco)
                .ToListAsync();
        }

        public async Task<Cliente?> GetByDocumentAsync(string document)
        {
            var documentWithoutMask = document
                .Replace(".", "")
                .Replace("-", "")
                .Replace("/", "")
                .Replace(" ", "");

            return await _context.Cliente
                .Include(c => c.Endereco)
                .FirstOrDefaultAsync(c => c.Documento
                    .Replace(".", "")
                    .Replace("-", "")
                    .Replace("/", "")
                    .Replace(" ", "") == documentWithoutMask);
        }

        #endregion
    }
}
