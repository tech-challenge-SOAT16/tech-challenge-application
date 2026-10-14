using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TechChallenge.Infrastructure.Database.Migrations
{
    /// <inheritdoc />
    public partial class UseSingleClienteDocumento : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Cliente_Cnpj",
                table: "Cliente");

            migrationBuilder.DropIndex(
                name: "IX_Cliente_Cpf",
                table: "Cliente");

            migrationBuilder.AddColumn<string>(
                name: "Documento",
                table: "Cliente",
                type: "character varying(18)",
                maxLength: 18,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "TipoDocumento",
                table: "Cliente",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.Sql("""
                UPDATE "Cliente"
                SET
                    "TipoDocumento" = CASE
                        WHEN "Cnpj" IS NOT NULL AND "Cnpj" <> '' THEN 1
                        WHEN "Cpf" IS NOT NULL AND "Cpf" <> '' THEN 0
                        ELSE 2
                    END,
                    "Documento" = COALESCE(NULLIF("Cnpj", ''), NULLIF("Cpf", ''), NULLIF("Rg", ''), "Id"::text);
                """);

            migrationBuilder.AlterColumn<string>(
                name: "Documento",
                table: "Cliente",
                type: "character varying(18)",
                maxLength: 18,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(18)",
                oldMaxLength: 18,
                oldNullable: true);

            migrationBuilder.DropColumn(
                name: "Cnpj",
                table: "Cliente");

            migrationBuilder.DropColumn(
                name: "Cpf",
                table: "Cliente");

            migrationBuilder.DropColumn(
                name: "Rg",
                table: "Cliente");

            migrationBuilder.CreateIndex(
                name: "IX_Cliente_Documento",
                table: "Cliente",
                column: "Documento",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Cliente_Documento",
                table: "Cliente");

            migrationBuilder.DropColumn(
                name: "Documento",
                table: "Cliente");

            migrationBuilder.DropColumn(
                name: "TipoDocumento",
                table: "Cliente");

            migrationBuilder.AddColumn<string>(
                name: "Cnpj",
                table: "Cliente",
                type: "character varying(18)",
                maxLength: 18,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Cpf",
                table: "Cliente",
                type: "character varying(14)",
                maxLength: 14,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Rg",
                table: "Cliente",
                type: "character varying(9)",
                maxLength: 9,
                nullable: false,
                defaultValue: "");

            migrationBuilder.Sql("""
                UPDATE "Cliente"
                SET
                    "Cpf" = CASE WHEN "TipoDocumento" = 0 THEN "Documento" ELSE NULL END,
                    "Cnpj" = CASE WHEN "TipoDocumento" = 1 THEN "Documento" ELSE NULL END,
                    "Rg" = CASE WHEN "TipoDocumento" = 2 THEN "Documento" ELSE '' END;
                """);

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
    }
}
