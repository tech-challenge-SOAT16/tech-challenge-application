using Microsoft.EntityFrameworkCore;
using TechChallenge.Application.Abstractions.Repositories;
using TechChallenge.Domain.Entities;
using TechChallenge.Infrastructure.Database.Context;

namespace TechChallenge.Infrastructure.Database.Repositories;

public class NotificacaoStatusOutboxRepository : INotificacaoStatusOutboxRepository
{
    private readonly ApplicationDbContext _context;

    public NotificacaoStatusOutboxRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IReadOnlyList<NotificacaoStatusOutbox>> ReservarPendentesAsync(
        DateTime agora,
        int quantidade,
        TimeSpan duracaoBloqueio,
        CancellationToken cancellationToken = default)
    {
        var notificacoes = await _context.NotificacaoStatusOutbox
            .Include(item => item.Cliente)
            .Where(item => item.EnviadaEm == null &&
                           item.ProximaTentativaEm <= agora &&
                           (item.BloqueadaAte == null || item.BloqueadaAte <= agora))
            .OrderBy(item => item.CriadaEm)
            .Take(quantidade)
            .ToListAsync(cancellationToken);

        foreach (var notificacao in notificacoes)
            notificacao.Reservar(agora, duracaoBloqueio);

        try
        {
            await _context.SaveChangesAsync(cancellationToken);
            return notificacoes;
        }
        catch (DbUpdateConcurrencyException)
        {
            _context.ChangeTracker.Clear();
            return [];
        }
    }

    public async Task SalvarAsync(CancellationToken cancellationToken = default)
    {
        await _context.SaveChangesAsync(cancellationToken);
    }
}
