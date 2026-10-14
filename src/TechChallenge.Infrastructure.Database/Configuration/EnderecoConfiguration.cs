using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TechChallenge.Domain.Entities;

namespace TechChallenge.Infrastructure.Database.Configuration
{
    public class EnderecoConfiguration : IEntityTypeConfiguration<Endereco>
    {
        public void Configure(EntityTypeBuilder<Endereco> builder)
        {
            builder.HasKey(e => e.Id);

            builder.Property(e => e.Logradouro)
                .HasMaxLength(100)
                .IsRequired();

            builder.Property(e => e.Complemento)
                .HasMaxLength(80);

            builder.Property(e => e.Numero)
                .HasMaxLength(10)
                .IsRequired();

            builder.Property(e => e.Bairro)
                .HasMaxLength(50)
                .IsRequired();

            builder.Property(e => e.Cidade)
                .HasMaxLength(50)
                .IsRequired();

            builder.Property(e => e.Estado)
                .HasMaxLength(30)
                .IsRequired();

            builder.Property(e => e.Cep)
                .HasMaxLength(30)
                .IsRequired();
        }
    }
}