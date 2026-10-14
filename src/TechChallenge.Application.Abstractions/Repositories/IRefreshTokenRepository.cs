using System;
using TechChallenge.Domain.Entities;

namespace TechChallenge.Application.Abstractions.Repositories
{
    public interface IRefreshTokenRepository : IRepository<RefreshToken>
    {
        Task<RefreshToken?> GetByHashAsync(string hash);
        Task<List<RefreshToken>> GetAtivasDoUsuarioAsync(Guid usuarioId, DateTime agora);
        Task RevogarTodasDoUsuarioAsync(Guid usuarioId, DateTime agora);
    }
}
