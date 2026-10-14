using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TechChallenge.Infrastructure.Database.Migrations
{
    /// <inheritdoc />
    public partial class AddCnpjStockTrackingAndOsMetrics : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Cliente_Cpf",
                table: "Cliente");

            migrationBuilder.AddColumn<int>(
                name: "Quantidade",
                table: "Produto",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "Quantidade",
                table: "OrdemServicoServicos",
                type: "integer",
                nullable: false,
                defaultValue: 1);

            migrationBuilder.AddColumn<int>(
                name: "Quantidade",
                table: "OrdemServicoProdutos",
                type: "integer",
                nullable: false,
                defaultValue: 1);

            migrationBuilder.AddColumn<string>(
                name: "CodigoAcompanhamento",
                table: "OrdemServico",
                type: "character varying(32)",
                maxLength: 32,
                nullable: true);

            migrationBuilder.Sql("""
                UPDATE "OrdemServico"
                SET "CodigoAcompanhamento" = replace("Id"::text, '-', '')
                WHERE "CodigoAcompanhamento" IS NULL OR "CodigoAcompanhamento" = '';
                """);

            migrationBuilder.AlterColumn<string>(
                name: "CodigoAcompanhamento",
                table: "OrdemServico",
                type: "character varying(32)",
                maxLength: 32,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(32)",
                oldMaxLength: 32,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Cpf",
                table: "Cliente",
                type: "character varying(14)",
                maxLength: 14,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(14)",
                oldMaxLength: 14);

            migrationBuilder.AddColumn<string>(
                name: "Cnpj",
                table: "Cliente",
                type: "character varying(18)",
                maxLength: 18,
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_OrdemServico_CodigoAcompanhamento",
                table: "OrdemServico",
                column: "CodigoAcompanhamento",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Cliente_Cnpj",
                table: "Cliente",
                column: "Cnpj",
                unique: true,
                filter: "\"Cnpj\" IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Cliente_Cpf",
                table: "Cliente",
                column: "Cpf",
                unique: true,
                filter: "\"Cpf\" IS NOT NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_OrdemServico_CodigoAcompanhamento",
                table: "OrdemServico");

            migrationBuilder.DropIndex(
                name: "IX_Cliente_Cnpj",
                table: "Cliente");

            migrationBuilder.DropIndex(
                name: "IX_Cliente_Cpf",
                table: "Cliente");

            migrationBuilder.DropColumn(
                name: "Quantidade",
                table: "Produto");

            migrationBuilder.DropColumn(
                name: "Quantidade",
                table: "OrdemServicoServicos");

            migrationBuilder.DropColumn(
                name: "Quantidade",
                table: "OrdemServicoProdutos");

            migrationBuilder.DropColumn(
                name: "CodigoAcompanhamento",
                table: "OrdemServico");

            migrationBuilder.DropColumn(
                name: "Cnpj",
                table: "Cliente");

            migrationBuilder.AlterColumn<string>(
                name: "Cpf",
                table: "Cliente",
                type: "character varying(14)",
                maxLength: 14,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "character varying(14)",
                oldMaxLength: 14,
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Cliente_Cpf",
                table: "Cliente",
                column: "Cpf",
                unique: true);
        }
    }
}
