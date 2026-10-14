using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TechChallenge.Infrastructure.Database.Migrations
{
    /// <inheritdoc />
    public partial class AddPhase2InventoryCategories : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "CategoriaId",
                table: "Veiculo",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000103"));

            migrationBuilder.AddColumn<Guid>(
                name: "CategoriaId",
                table: "Servico",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000102"));

            migrationBuilder.AddColumn<Guid>(
                name: "CategoriaId",
                table: "Produto",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000101"));

            migrationBuilder.CreateTable(
                name: "CategoriaProduto",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Descricao = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CategoriaProduto", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "CategoriaServico",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Descricao = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CategoriaServico", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "CategoriaVeiculo",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Descricao = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CategoriaVeiculo", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Estoque",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ProdutoId = table.Column<Guid>(type: "uuid", nullable: false),
                    Quantidade = table.Column<int>(type: "integer", nullable: false),
                    Versao = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Estoque", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Estoque_Produto_ProdutoId",
                        column: x => x.ProdutoId,
                        principalTable: "Produto",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.InsertData(
                table: "CategoriaProduto",
                columns: new[] { "Id", "Descricao" },
                values: new object[] { new Guid("00000000-0000-0000-0000-000000000101"), "Categoria legada" });

            migrationBuilder.InsertData(
                table: "CategoriaServico",
                columns: new[] { "Id", "Descricao" },
                values: new object[] { new Guid("00000000-0000-0000-0000-000000000102"), "Categoria legada" });

            migrationBuilder.InsertData(
                table: "CategoriaVeiculo",
                columns: new[] { "Id", "Descricao" },
                values: new object[] { new Guid("00000000-0000-0000-0000-000000000103"), "Categoria legada" });

            migrationBuilder.Sql(
                """
                INSERT INTO "Estoque" ("Id", "ProdutoId", "Quantidade", "Versao")
                SELECT "Id", "Id", "Quantidade", "Id"
                FROM "Produto";
                """);

            migrationBuilder.DropColumn(
                name: "Tipo",
                table: "Veiculo");

            migrationBuilder.DropColumn(
                name: "Quantidade",
                table: "Produto");

            migrationBuilder.CreateIndex(
                name: "IX_Veiculo_CategoriaId",
                table: "Veiculo",
                column: "CategoriaId");

            migrationBuilder.CreateIndex(
                name: "IX_Servico_CategoriaId",
                table: "Servico",
                column: "CategoriaId");

            migrationBuilder.CreateIndex(
                name: "IX_Produto_CategoriaId",
                table: "Produto",
                column: "CategoriaId");

            migrationBuilder.CreateIndex(
                name: "IX_Estoque_ProdutoId",
                table: "Estoque",
                column: "ProdutoId",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Produto_CategoriaProduto_CategoriaId",
                table: "Produto",
                column: "CategoriaId",
                principalTable: "CategoriaProduto",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Servico_CategoriaServico_CategoriaId",
                table: "Servico",
                column: "CategoriaId",
                principalTable: "CategoriaServico",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Veiculo_CategoriaVeiculo_CategoriaId",
                table: "Veiculo",
                column: "CategoriaId",
                principalTable: "CategoriaVeiculo",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Produto_CategoriaProduto_CategoriaId",
                table: "Produto");

            migrationBuilder.DropForeignKey(
                name: "FK_Servico_CategoriaServico_CategoriaId",
                table: "Servico");

            migrationBuilder.DropForeignKey(
                name: "FK_Veiculo_CategoriaVeiculo_CategoriaId",
                table: "Veiculo");

            migrationBuilder.DropIndex(
                name: "IX_Veiculo_CategoriaId",
                table: "Veiculo");

            migrationBuilder.DropIndex(
                name: "IX_Servico_CategoriaId",
                table: "Servico");

            migrationBuilder.DropIndex(
                name: "IX_Produto_CategoriaId",
                table: "Produto");

            migrationBuilder.AddColumn<int>(
                name: "Tipo",
                table: "Veiculo",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "Quantidade",
                table: "Produto",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.Sql(
                """
                UPDATE "Produto" AS p
                SET "Quantidade" = e."Quantidade"
                FROM "Estoque" AS e
                WHERE e."ProdutoId" = p."Id";
                """);

            migrationBuilder.DropTable(
                name: "Estoque");

            migrationBuilder.DropTable(
                name: "CategoriaProduto");

            migrationBuilder.DropTable(
                name: "CategoriaServico");

            migrationBuilder.DropTable(
                name: "CategoriaVeiculo");

            migrationBuilder.DropColumn(
                name: "CategoriaId",
                table: "Veiculo");

            migrationBuilder.DropColumn(
                name: "CategoriaId",
                table: "Servico");

            migrationBuilder.DropColumn(
                name: "CategoriaId",
                table: "Produto");

        }
    }
}
