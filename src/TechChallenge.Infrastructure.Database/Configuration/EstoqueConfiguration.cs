using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TechChallenge.Domain.Entities;

namespace TechChallenge.Infrastructure.Database.Configuration
{
    public class EstoqueConfiguration : IEntityTypeConfiguration<Estoque>
    {
        public void Configure(EntityTypeBuilder<Estoque> builder)
        {
            builder.HasKey(c => c.Id);

            builder.HasOne(v => v.Produto)
                .WithOne(p => p.Estoque)
                .HasForeignKey<Estoque>(v => v.ProdutoId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasIndex(p => p.ProdutoId)
                .IsUnique();

            builder.Property(p => p.Quantidade)
                .IsRequired();

            builder.Property(p => p.Versao)
                .IsConcurrencyToken()
                .IsRequired();
        }
    }
}
