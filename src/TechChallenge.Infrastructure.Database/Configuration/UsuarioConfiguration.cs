using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TechChallenge.Domain.Entities;

namespace TechChallenge.Infrastructure.Database.Configuration
{
    public class UsuarioConfiguration : IEntityTypeConfiguration<Usuario>
    {
        public void Configure(EntityTypeBuilder<Usuario> builder)
        {
            builder.HasKey(u => u.Id);

            builder.Property(u => u.Login)
                .HasMaxLength(100)
                .IsRequired();

            builder.HasIndex(u => u.Login)
                .IsUnique();

            builder.Property(u => u.PasswordHash)
                .IsRequired();

            builder.Property(u => u.TipoUsuario)
                .HasConversion<byte>()
                .IsRequired();

            builder.Property(u => u.Ativo)
                .IsRequired();

            // Vínculo opcional com Funcionario, sem FK obrigatória; único quando preenchido.
            builder.HasOne(u => u.Funcionario)
                .WithMany()
                .HasForeignKey(u => u.FuncionarioId)
                .IsRequired(false);

            builder.HasIndex(u => u.FuncionarioId)
                .IsUnique()
                .HasFilter(null);

            builder.HasMany(u => u.RefreshTokens)
                .WithOne(t => t.Usuario)
                .HasForeignKey(t => t.UsuarioId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
