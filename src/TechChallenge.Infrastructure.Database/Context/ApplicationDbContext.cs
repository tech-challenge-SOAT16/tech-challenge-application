using Microsoft.EntityFrameworkCore;
using TechChallenge.Application.Abstractions.Observability;
using TechChallenge.Domain.Entities;

namespace TechChallenge.Infrastructure.Database.Context
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {

        }

        public DbSet<Cliente> Cliente { get; set; }
        public DbSet<Endereco> Endereco { get; set; }
        public DbSet<Funcionario> Funcionario { get; set; }
        public DbSet<OrdemServico> OrdemServico { get; set; }
        public DbSet<Produto> Produto { get; set; }
        public DbSet<Servico> Servico { get; set; }
        public DbSet<Veiculo> Veiculo { get; set; }
        public DbSet<Usuario> Usuario { get; set; }
        public DbSet<RefreshToken> RefreshToken { get; set; }
        public DbSet<OrdemServicoProdutos> OrdemServicoProdutos { get; set; }
        public DbSet<OrdemServicoServicos> OrdemServicoServicos { get; set; }
        public DbSet<CategoriaProduto> CategoriaProduto { get; set; }
        public DbSet<CategoriaServico> CategoriaServico { get; set; }
        public DbSet<CategoriaVeiculo> CategoriaVeiculo { get; set; }
        public DbSet<Estoque> Estoque { get; set; }
        public DbSet<DecisaoOrcamentoExterna> DecisaoOrcamentoExterna { get; set; }
        public DbSet<NotificacaoStatusOutbox> NotificacaoStatusOutbox { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);
        }

        public override int SaveChanges(bool acceptAllChangesOnSuccess)
        {
            var ordensComEventos = PrepararOutbox();
            var transicoes = CalcularTransicoes(ordensComEventos, ObterUltimasTransicoes(ordensComEventos));
            var resultado = base.SaveChanges(acceptAllChangesOnSuccess);
            LimparEventos(ordensComEventos);
            RegistrarMetricas(transicoes);
            return resultado;
        }

        public override async Task<int> SaveChangesAsync(
            bool acceptAllChangesOnSuccess,
            CancellationToken cancellationToken = default)
        {
            var ordensComEventos = PrepararOutbox();
            var transicoes = CalcularTransicoes(
                ordensComEventos,
                await ObterUltimasTransicoesAsync(ordensComEventos, cancellationToken));
            var resultado = await base.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken);
            LimparEventos(ordensComEventos);
            RegistrarMetricas(transicoes);
            return resultado;
        }

        private List<OrdemServico> PrepararOutbox()
        {
            var ordensComEventos = ChangeTracker.Entries<OrdemServico>()
                .Select(entry => entry.Entity)
                .Where(ordem => ordem.EventosDominio.Count > 0)
                .ToList();

            foreach (var evento in ordensComEventos.SelectMany(ordem => ordem.EventosDominio))
            {
                if (NotificacaoStatusOutbox.Local.Any(item => item.EventoId == evento.EventoId))
                    continue;

                NotificacaoStatusOutbox.Add(new NotificacaoStatusOutbox(
                    Guid.NewGuid(),
                    evento));
            }

            return ordensComEventos;
        }

        // A outbox guarda uma linha por transição; a mais recente marca a entrada no status atual.
        private IQueryable<UltimaTransicao> ConsultarUltimasTransicoes(List<OrdemServico> ordens)
        {
            var ids = ordens.Select(ordem => ordem.Id).ToList();
            return NotificacaoStatusOutbox.AsNoTracking()
                .Where(item => ids.Contains(item.OrdemServicoId))
                .GroupBy(item => item.OrdemServicoId)
                .Select(grupo => new UltimaTransicao(grupo.Key, grupo.Max(item => item.CriadaEm)));
        }

        private Dictionary<Guid, DateTime> ObterUltimasTransicoes(List<OrdemServico> ordens) =>
            ordens.Count == 0
                ? []
                : ConsultarUltimasTransicoes(ordens).ToDictionary(item => item.OrdemServicoId, item => item.CriadaEm);

        private async Task<Dictionary<Guid, DateTime>> ObterUltimasTransicoesAsync(
            List<OrdemServico> ordens,
            CancellationToken cancellationToken) =>
            ordens.Count == 0
                ? []
                : await ConsultarUltimasTransicoes(ordens)
                    .ToDictionaryAsync(item => item.OrdemServicoId, item => item.CriadaEm, cancellationToken);

        private sealed record UltimaTransicao(Guid OrdemServicoId, DateTime CriadaEm);

        private static List<(string Anterior, string Atual, double Segundos)> CalcularTransicoes(
            List<OrdemServico> ordens,
            Dictionary<Guid, DateTime> ultimasTransicoes)
        {
            var transicoes = new List<(string, string, double)>();
            foreach (var ordem in ordens)
            {
                var inicio = ultimasTransicoes.TryGetValue(ordem.Id, out var ultima) ? ultima : ordem.DataCriacao;
                foreach (var evento in ordem.EventosDominio.OrderBy(item => item.OcorridoEm))
                {
                    var segundos = Math.Max(0, (evento.OcorridoEm - inicio).TotalSeconds);
                    transicoes.Add((evento.StatusAnterior.ToString(), evento.StatusAtual.ToString(), segundos));
                    inicio = evento.OcorridoEm;
                }
            }

            return transicoes;
        }

        private static void RegistrarMetricas(List<(string Anterior, string Atual, double Segundos)> transicoes)
        {
            foreach (var (anterior, atual, segundos) in transicoes)
            {
                TechChallengeMetrics.Transicoes.Add(
                    1,
                    new KeyValuePair<string, object?>("status_anterior", anterior),
                    new KeyValuePair<string, object?>("status_atual", atual));
                TechChallengeMetrics.TempoNoStatus.Record(
                    segundos,
                    new KeyValuePair<string, object?>("status", anterior));
            }
        }

        private static void LimparEventos(IEnumerable<OrdemServico> ordens)
        {
            foreach (var ordem in ordens)
                ordem.LimparEventosDominio();
        }
    }
}
