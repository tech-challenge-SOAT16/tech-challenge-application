using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TechChallenge.Domain.Entities;

namespace TechChallenge.Infrastructure.Database.Configuration
{
    public class OrdemServicoProdutosConfiguration : IEntityTypeConfiguration<OrdemServicoProdutos>
    {
        public void Configure(EntityTypeBuilder<OrdemServicoProdutos> builder)
        {
            builder.HasKey(osp => osp.Id);

            builder.HasOne(osp => osp.OrdemServico)
                .WithMany(os => os.Produtos)
                .HasForeignKey(osp => osp.OrdemServicoId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(osp => osp.Produto)
                .WithMany()
                .HasForeignKey(osp => osp.ProdutoId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Property(osp => osp.Quantidade)
                .IsRequired();
        }
    }
}
