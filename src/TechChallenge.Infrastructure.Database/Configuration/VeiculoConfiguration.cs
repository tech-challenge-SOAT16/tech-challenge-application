using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TechChallenge.Domain.Entities;

namespace TechChallenge.Infrastructure.Database.Configuration
{
    public class VeiculoConfiguration : IEntityTypeConfiguration<Veiculo>
    {
        public void Configure(EntityTypeBuilder<Veiculo> builder)
        {
            builder.HasKey(v => v.Id);

            builder.Property(v => v.Placa)
                .HasMaxLength(10)
                .IsRequired();

            builder.HasIndex(v => v.Placa)
                .IsUnique();

            builder.Property(v => v.Modelo)
                .HasMaxLength(30);

            builder.Property(v => v.Marca)
                .HasMaxLength(30);

            builder.Property(v => v.Cor)
                .HasMaxLength(20);

            builder.Property(v => v.Ano)
                .IsRequired();

            builder.Property(v => v.Quilometragem)
                .HasPrecision(18, 2)
                .IsRequired();

            builder.Property(v => v.Valor)
                .HasPrecision(18, 2)
                .IsRequired();

            builder.HasOne(v => v.ClienteResponsavel)
                .WithMany()
                .HasForeignKey(v => v.ClienteId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(v => v.Categoria)
                .WithMany()
                .HasForeignKey(v => v.CategoriaId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
