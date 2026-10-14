using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TechChallenge.Domain.Entities;

namespace TechChallenge.Infrastructure.Database.Configuration
{
    public class RefreshTokenConfiguration : IEntityTypeConfiguration<RefreshToken>
    {
        public void Configure(EntityTypeBuilder<RefreshToken> builder)
        {
            builder.HasKey(t => t.Id);

            builder.Property(t => t.TokenHash)
                .HasMaxLength(100)
                .IsRequired();

            builder.HasIndex(t => t.TokenHash)
                .IsUnique();

            builder.Property(t => t.CriadoEm).IsRequired();
            builder.Property(t => t.ExpiraEm).IsRequired();
        }
    }
}
