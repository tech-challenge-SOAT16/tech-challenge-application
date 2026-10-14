using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TechChallenge.Infrastructure.Database.Migrations
{
    /// <inheritdoc />
    public partial class AddExternalDecisionAndStatusNotifications : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Email",
                table: "Cliente",
                type: "character varying(254)",
                maxLength: 254,
                nullable: true);

            migrationBuilder.Sql(
                """
                UPDATE "Cliente"
                SET "Email" = 'cliente+' || replace("Id"::text, '-', '') || '@oficina.local'
                WHERE "Email" IS NULL;
                """);

            migrationBuilder.AlterColumn<string>(
                name: "Email",
                table: "Cliente",
                type: "character varying(254)",
                maxLength: 254,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(254)",
                oldMaxLength: 254,
                oldNullable: true);

            migrationBuilder.CreateTable(
                name: "DecisaoOrcamentoExterna",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    OrdemServicoId = table.Column<Guid>(type: "uuid", nullable: false),
                    EventoId = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Decisao = table.Column<int>(type: "integer", nullable: false),
                    Motivo = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    OcorridoEm = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    RecebidoEm = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DecisaoOrcamentoExterna", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DecisaoOrcamentoExterna_OrdemServico_OrdemServicoId",
                        column: x => x.OrdemServicoId,
                        principalTable: "OrdemServico",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "NotificacaoStatusOutbox",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    EventoId = table.Column<Guid>(type: "uuid", nullable: false),
                    OrdemServicoId = table.Column<Guid>(type: "uuid", nullable: false),
                    ClienteId = table.Column<Guid>(type: "uuid", nullable: false),
                    CodigoAcompanhamento = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    StatusAnterior = table.Column<int>(type: "integer", nullable: false),
                    StatusAtual = table.Column<int>(type: "integer", nullable: false),
                    CriadaEm = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    EnviadaEm = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ProximaTentativaEm = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    BloqueadaAte = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Tentativas = table.Column<int>(type: "integer", nullable: false),
                    UltimoErro = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    Versao = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NotificacaoStatusOutbox", x => x.Id);
                    table.ForeignKey(
                        name: "FK_NotificacaoStatusOutbox_Cliente_ClienteId",
                        column: x => x.ClienteId,
                        principalTable: "Cliente",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_DecisaoOrcamentoExterna_EventoId",
                table: "DecisaoOrcamentoExterna",
                column: "EventoId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_DecisaoOrcamentoExterna_OrdemServicoId",
                table: "DecisaoOrcamentoExterna",
                column: "OrdemServicoId");

            migrationBuilder.CreateIndex(
                name: "IX_NotificacaoStatusOutbox_ClienteId",
                table: "NotificacaoStatusOutbox",
                column: "ClienteId");

            migrationBuilder.CreateIndex(
                name: "IX_NotificacaoStatusOutbox_EnviadaEm_ProximaTentativaEm_Bloque~",
                table: "NotificacaoStatusOutbox",
                columns: new[] { "EnviadaEm", "ProximaTentativaEm", "BloqueadaAte" });

            migrationBuilder.CreateIndex(
                name: "IX_NotificacaoStatusOutbox_EventoId",
                table: "NotificacaoStatusOutbox",
                column: "EventoId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DecisaoOrcamentoExterna");

            migrationBuilder.DropTable(
                name: "NotificacaoStatusOutbox");

            migrationBuilder.DropColumn(
                name: "Email",
                table: "Cliente");
        }
    }
}
