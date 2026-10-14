using TechChallenge.Domain.Entities;

namespace TechChallenge.Application.Abstractions.Repositories;

public interface INotificacaoStatusOutboxRepository
{
    Task<IReadOnlyList<NotificacaoStatusOutbox>> ReservarPendentesAsync(
        DateTime agora,
        int quantidade,
        TimeSpan duracaoBloqueio,
        CancellationToken cancellationToken = default);

    Task SalvarAsync(CancellationToken cancellationToken = default);
}
