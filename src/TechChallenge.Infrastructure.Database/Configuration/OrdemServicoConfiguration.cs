using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TechChallenge.Domain.Entities;

namespace TechChallenge.Infrastructure.Database.Configuration
{
    public class OrdemServicoConfiguration : IEntityTypeConfiguration<OrdemServico>
    {
        public void Configure(EntityTypeBuilder<OrdemServico> builder)
        {
            builder.HasKey(os => os.Id);

            builder.Ignore(os => os.EventosDominio);

            builder.Property(o => o.Descricao)
                .HasMaxLength(500);

            builder.Property(os => os.CodigoAcompanhamento)
                .HasMaxLength(32)
                .IsRequired();

            builder.HasIndex(os => os.CodigoAcompanhamento)
                .IsUnique();

            builder.HasOne(os => os.ClienteResponsavel)
                .WithMany()
                .HasForeignKey(os => os.ClienteResponsavelId)
                .IsRequired()
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(os => os.FuncionarioResponsavel)
                .WithMany()
                .HasForeignKey(os => os.FuncionarioResponsavelId)
                .IsRequired()
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(os => os.Veiculo)
                .WithMany()
                .HasForeignKey(os => os.VeiculoId)
                .IsRequired()
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasMany(os => os.DecisoesExternas)
                .WithOne()
                .HasForeignKey(item => item.OrdemServicoId)
                .IsRequired()
                .OnDelete(DeleteBehavior.Cascade);

            builder.Property(os => os.Status)
                .IsRequired();

            builder.Property(os => os.DataCriacao)
                .IsRequired();

            builder.Property(os => os.DataAtualizacao)
                .IsRequired(false);

            builder.Property(os => os.DataFinalizacao)
                .IsRequired(false);

            builder.Property(os => os.Valor);
            builder.Property(os => os.Desconto);
            builder.Property(os => os.Acrescimo);
        }
    }
}
