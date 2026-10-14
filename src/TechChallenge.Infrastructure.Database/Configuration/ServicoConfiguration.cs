using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TechChallenge.Domain.Entities;

namespace TechChallenge.Infrastructure.Database.Configuration
{
    public class ServicoConfiguration : IEntityTypeConfiguration<Servico>
    {
        public void Configure(EntityTypeBuilder<Servico> builder)
        {
            builder.HasKey(s => s.Id);

            builder.Property(s => s.Descricao)
                .HasMaxLength(100)
                .IsRequired();

            builder.Property(s => s.Valor)
                .HasPrecision(18, 2)
                .IsRequired();

            builder.HasOne(v => v.Categoria)
                .WithMany()
                .HasForeignKey(v => v.CategoriaId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}