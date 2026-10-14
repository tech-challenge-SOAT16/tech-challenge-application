using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TechChallenge.Domain.Entities;

namespace TechChallenge.Infrastructure.Database.Configuration
{
    public class FuncionarioConfiguration : IEntityTypeConfiguration<Funcionario>
    {
        public void Configure(EntityTypeBuilder<Funcionario> builder)
        {
            builder.HasKey(f => f.Id);

            builder.Property(f => f.Nome)
                .HasMaxLength(100)
                .IsRequired();

            builder.Property(f => f.Cpf)
                .HasMaxLength(14)
                .IsRequired();

            builder.HasIndex(f => f.Cpf)
                .IsUnique();

            builder.Property(f => f.Rg)
                .HasMaxLength(9)
                .IsRequired();

            builder.Property(f => f.TipoFuncionario)
                .HasConversion<byte>()
                .IsRequired();

            builder.HasOne(f => f.Endereco)
                .WithMany()
                .HasForeignKey(f => f.EnderecoId)
                .IsRequired()
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
