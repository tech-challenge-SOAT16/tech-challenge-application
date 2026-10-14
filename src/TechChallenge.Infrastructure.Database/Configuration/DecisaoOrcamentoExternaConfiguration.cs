using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TechChallenge.Domain.Entities;

namespace TechChallenge.Infrastructure.Database.Configuration;

public class DecisaoOrcamentoExternaConfiguration : IEntityTypeConfiguration<DecisaoOrcamentoExterna>
{
    public void Configure(EntityTypeBuilder<DecisaoOrcamentoExterna> builder)
    {
        builder.HasKey(item => item.Id);

        builder.Property(item => item.EventoId)
            .HasMaxLength(100)
            .IsRequired();

        builder.HasIndex(item => item.EventoId)
            .IsUnique();

        builder.Property(item => item.Decisao)
            .IsRequired();

        builder.Property(item => item.Motivo)
            .HasMaxLength(500);

        builder.Property(item => item.OcorridoEm)
            .IsRequired();

        builder.Property(item => item.RecebidoEm)
            .IsRequired();
    }
}
