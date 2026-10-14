using System;
using Microsoft.EntityFrameworkCore;
using TechChallenge.Domain.Entities;
using TechChallenge.Application.Abstractions.Repositories;
using TechChallenge.Infrastructure.Database.Context;

namespace TechChallenge.Infrastructure.Database.Repositories
{
    public class FuncionarioRepository : Repository<Funcionario>, IFuncionarioRepository
    {
        #region Properties

        private readonly ApplicationDbContext _context;

        #endregion

        #region Constructor

        public FuncionarioRepository(ApplicationDbContext context) : base(context)
        {
            _context = context;
        }

        #endregion

        #region Members of IFuncionarioRepository

        public override async Task<Funcionario?> GetByIdAsync(Guid id)
        {
            return await _context.Funcionario
                .Include(f => f.Endereco)
                .FirstOrDefaultAsync(f => f.Id == id);
        }

        public override async Task<List<Funcionario>> GetAllAsync()
        {
            return await _context.Funcionario
                .Include(f => f.Endereco)
                .ToListAsync();
        }

        public async Task<Funcionario?> GetByDocumentAsync(string document)
        {
            var documentWithoutMask = document.Replace(".", "").Replace("-", "").Replace(" ", "");

            return await _context.Funcionario
                .Include(f => f.Endereco)
                .FirstOrDefaultAsync(f => f.Cpf.Replace(".", "").Replace("-", "").Replace(" ", "") == documentWithoutMask);
        }

        #endregion
    }
}
