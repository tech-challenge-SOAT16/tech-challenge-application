using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TechChallenge.Domain.Entities;

namespace TechChallenge.Infrastructure.Database.Configuration
{
    public class ClienteConfiguration : IEntityTypeConfiguration<Cliente>
    {
        public void Configure(EntityTypeBuilder<Cliente> builder)
        {
            builder.HasKey(c => c.Id);

            builder.Property(c => c.Nome)
                .HasMaxLength(100)
                .IsRequired();

            builder.Property(c => c.Email)
                .HasMaxLength(254)
                .IsRequired();

            builder.Property(c => c.TipoDocumento)
                .IsRequired();

            builder.Property(c => c.Documento)
                .IsRequired()
                .HasMaxLength(18);

            builder.HasIndex(c => c.Documento)
                .IsUnique();

            builder.HasOne(f => f.Endereco)
                .WithMany()
                .HasForeignKey(f => f.EnderecoId)
                .IsRequired()
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
