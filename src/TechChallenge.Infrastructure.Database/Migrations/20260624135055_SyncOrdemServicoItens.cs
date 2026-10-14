using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TechChallenge.Infrastructure.Database.Migrations
{
    /// <inheritdoc />
    public partial class SyncOrdemServicoItens : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                DROP TABLE IF EXISTS "OrdemServicoProduto";
                DROP TABLE IF EXISTS "OrdemServicoServico";

                ALTER TABLE "OrdemServico" ADD COLUMN IF NOT EXISTS "Acrescimo" double precision NOT NULL DEFAULT 0.0;
                ALTER TABLE "OrdemServico" ADD COLUMN IF NOT EXISTS "Desconto" double precision NOT NULL DEFAULT 0.0;
                ALTER TABLE "OrdemServico" ADD COLUMN IF NOT EXISTS "Valor" double precision NOT NULL DEFAULT 0.0;

                CREATE TABLE IF NOT EXISTS "OrdemServicoProdutos" (
                    "Id" uuid NOT NULL,
                    "OrdemServicoId" uuid NOT NULL,
                    "ProdutoId" uuid NOT NULL,
                    "Valor" double precision NOT NULL,
                    "Desconto" double precision NOT NULL,
                    "Acrescimo" double precision NOT NULL,
                    CONSTRAINT "PK_OrdemServicoProdutos" PRIMARY KEY ("Id"),
                    CONSTRAINT "FK_OrdemServicoProdutos_OrdemServico_OrdemServicoId" FOREIGN KEY ("OrdemServicoId") REFERENCES "OrdemServico" ("Id") ON DELETE CASCADE,
                    CONSTRAINT "FK_OrdemServicoProdutos_Produto_ProdutoId" FOREIGN KEY ("ProdutoId") REFERENCES "Produto" ("Id") ON DELETE CASCADE
                );

                CREATE TABLE IF NOT EXISTS "OrdemServicoServicos" (
                    "Id" uuid NOT NULL,
                    "OrdemServicoId" uuid NOT NULL,
                    "ServicoId" uuid NOT NULL,
                    "Valor" double precision NOT NULL,
                    "Desconto" double precision NOT NULL,
                    "Acrescimo" double precision NOT NULL,
                    CONSTRAINT "PK_OrdemServicoServicos" PRIMARY KEY ("Id"),
                    CONSTRAINT "FK_OrdemServicoServicos_OrdemServico_OrdemServicoId" FOREIGN KEY ("OrdemServicoId") REFERENCES "OrdemServico" ("Id") ON DELETE CASCADE,
                    CONSTRAINT "FK_OrdemServicoServicos_Servico_ServicoId" FOREIGN KEY ("ServicoId") REFERENCES "Servico" ("Id") ON DELETE CASCADE
                );

                CREATE INDEX IF NOT EXISTS "IX_OrdemServicoProdutos_OrdemServicoId" ON "OrdemServicoProdutos" ("OrdemServicoId");
                CREATE INDEX IF NOT EXISTS "IX_OrdemServicoProdutos_ProdutoId" ON "OrdemServicoProdutos" ("ProdutoId");
                CREATE INDEX IF NOT EXISTS "IX_OrdemServicoServicos_OrdemServicoId" ON "OrdemServicoServicos" ("OrdemServicoId");
                CREATE INDEX IF NOT EXISTS "IX_OrdemServicoServicos_ServicoId" ON "OrdemServicoServicos" ("ServicoId");
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                DROP TABLE IF EXISTS "OrdemServicoProdutos";
                DROP TABLE IF EXISTS "OrdemServicoServicos";

                ALTER TABLE "OrdemServico" DROP COLUMN IF EXISTS "Acrescimo";
                ALTER TABLE "OrdemServico" DROP COLUMN IF EXISTS "Desconto";
                ALTER TABLE "OrdemServico" DROP COLUMN IF EXISTS "Valor";

                CREATE TABLE IF NOT EXISTS "OrdemServicoProduto" (
                    "OrdemServicoId" uuid NOT NULL,
                    "ProdutosId" uuid NOT NULL,
                    CONSTRAINT "PK_OrdemServicoProduto" PRIMARY KEY ("OrdemServicoId", "ProdutosId"),
                    CONSTRAINT "FK_OrdemServicoProduto_OrdemServico_OrdemServicoId" FOREIGN KEY ("OrdemServicoId") REFERENCES "OrdemServico" ("Id") ON DELETE CASCADE,
                    CONSTRAINT "FK_OrdemServicoProduto_Produto_ProdutosId" FOREIGN KEY ("ProdutosId") REFERENCES "Produto" ("Id") ON DELETE CASCADE
                );

                CREATE TABLE IF NOT EXISTS "OrdemServicoServico" (
                    "OrdemServicoId" uuid NOT NULL,
                    "ServicosId" uuid NOT NULL,
                    CONSTRAINT "PK_OrdemServicoServico" PRIMARY KEY ("OrdemServicoId", "ServicosId"),
                    CONSTRAINT "FK_OrdemServicoServico_OrdemServico_OrdemServicoId" FOREIGN KEY ("OrdemServicoId") REFERENCES "OrdemServico" ("Id") ON DELETE CASCADE,
                    CONSTRAINT "FK_OrdemServicoServico_Servico_ServicosId" FOREIGN KEY ("ServicosId") REFERENCES "Servico" ("Id") ON DELETE CASCADE
                );

                CREATE INDEX IF NOT EXISTS "IX_OrdemServicoProduto_ProdutosId" ON "OrdemServicoProduto" ("ProdutosId");
                CREATE INDEX IF NOT EXISTS "IX_OrdemServicoServico_ServicosId" ON "OrdemServicoServico" ("ServicosId");
                """);
        }
    }
}
