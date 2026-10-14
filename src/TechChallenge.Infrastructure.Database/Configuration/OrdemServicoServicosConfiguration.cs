using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TechChallenge.Domain.Entities;

namespace TechChallenge.Infrastructure.Database.Configuration
{
    public class OrdemServicoServicosConfiguration : IEntityTypeConfiguration<OrdemServicoServicos>
    {
        public void Configure(EntityTypeBuilder<OrdemServicoServicos> builder)
        {
            builder.HasKey(oss => oss.Id);

            builder.HasOne(oss => oss.OrdemServico)
                .WithMany(os => os.Servicos)
                .HasForeignKey(oss => oss.OrdemServicoId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(oss => oss.Servico)
                .WithMany()
                .HasForeignKey(oss => oss.ServicoId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Property(oss => oss.Quantidade)
                .IsRequired();
        }
    }
}
