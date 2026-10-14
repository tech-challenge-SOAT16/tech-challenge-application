using Microsoft.EntityFrameworkCore;
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
            var resultado = base.SaveChanges(acceptAllChangesOnSuccess);
            LimparEventos(ordensComEventos);
            return resultado;
        }

        public override async Task<int> SaveChangesAsync(
            bool acceptAllChangesOnSuccess,
            CancellationToken cancellationToken = default)
        {
            var ordensComEventos = PrepararOutbox();
            var resultado = await base.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken);
            LimparEventos(ordensComEventos);
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

        private static void LimparEventos(IEnumerable<OrdemServico> ordens)
        {
            foreach (var ordem in ordens)
                ordem.LimparEventosDominio();
        }
    }
}
