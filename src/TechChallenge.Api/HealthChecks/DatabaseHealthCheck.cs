using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using TechChallenge.Infrastructure.Database.Context;

namespace TechChallenge.Api.HealthChecks;

public sealed class DatabaseHealthCheck(IServiceScopeFactory scopeFactory) : IHealthCheck
{
    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        using var scope = scopeFactory.CreateScope();
        var database = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        // Verifica a conexão e o acesso à tabela de usuários após a inicialização.
        await database.Usuario.AsNoTracking().AnyAsync(cancellationToken);
        return HealthCheckResult.Healthy();
    }
}
