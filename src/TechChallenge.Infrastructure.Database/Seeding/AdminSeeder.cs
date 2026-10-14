using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using TechChallenge.Application.Abstractions.Auth;
using TechChallenge.Domain.Entities;
using TechChallenge.Domain.Enums;
using TechChallenge.Infrastructure.Database.Context;

namespace TechChallenge.Infrastructure.Database.Seeding
{
    internal static class AdminSeeder
    {
        public static async Task SeedAsync(IServiceProvider provider, ApplicationDbContext context)
        {
            var hasher = provider.GetRequiredService<IPasswordHasher>();
            var configuration = provider.GetRequiredService<IConfiguration>();
            var login = configuration["Seed:AdminLogin"] ?? "admin";
            var senha = configuration["Seed:AdminPassword"];

            if (await context.Usuario.AnyAsync(usuario => usuario.Login == login))
                return;

            if (string.IsNullOrWhiteSpace(senha))
                return;

            context.Usuario.Add(new Usuario(Guid.NewGuid(), login, hasher.Hash(senha), TipoUsuario.Administrador, true));

            await context.SaveChangesAsync();
        }
    }
}
