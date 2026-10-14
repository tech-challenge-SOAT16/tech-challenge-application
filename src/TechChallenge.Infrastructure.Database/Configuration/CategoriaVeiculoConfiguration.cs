using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TechChallenge.Domain.Entities;

namespace TechChallenge.Infrastructure.Database.Configuration
{
    public class CategoriaVeiculoConfiguration : IEntityTypeConfiguration<CategoriaVeiculo>
    {
        public void Configure(EntityTypeBuilder<CategoriaVeiculo> builder)
        {
            builder.HasKey(c => c.Id);

            builder.Property(c => c.Descricao)
                .HasMaxLength(100)
                .IsRequired();
        }
    }
}