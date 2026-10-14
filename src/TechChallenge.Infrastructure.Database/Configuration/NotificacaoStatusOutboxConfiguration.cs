using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TechChallenge.Domain.Entities;

namespace TechChallenge.Infrastructure.Database.Configuration;

public class NotificacaoStatusOutboxConfiguration : IEntityTypeConfiguration<NotificacaoStatusOutbox>
{
    public void Configure(EntityTypeBuilder<NotificacaoStatusOutbox> builder)
    {
        builder.HasKey(item => item.Id);

        builder.HasIndex(item => item.EventoId)
            .IsUnique();

        builder.HasIndex(item => new { item.EnviadaEm, item.ProximaTentativaEm, item.BloqueadaAte });

        builder.Property(item => item.UltimoErro)
            .HasMaxLength(1000);

        builder.Property(item => item.CodigoAcompanhamento)
            .HasMaxLength(32)
            .IsRequired();

        builder.Property(item => item.Versao)
            .IsConcurrencyToken();

        builder.HasOne(item => item.Cliente)
            .WithMany()
            .HasForeignKey(item => item.ClienteId)
            .IsRequired()
            .OnDelete(DeleteBehavior.Restrict);
    }
}
