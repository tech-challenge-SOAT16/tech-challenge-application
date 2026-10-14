using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TechChallenge.Infrastructure.Database.Migrations
{
    /// <inheritdoc />
    public partial class SimplifyRefreshToken : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_RefreshToken_SessaoId",
                table: "RefreshToken");

            migrationBuilder.DropColumn(
                name: "IpCriacao",
                table: "RefreshToken");

            migrationBuilder.DropColumn(
                name: "MotivoRevogacao",
                table: "RefreshToken");

            migrationBuilder.DropColumn(
                name: "SessaoExpiraEm",
                table: "RefreshToken");

            migrationBuilder.DropColumn(
                name: "SessaoId",
                table: "RefreshToken");

            migrationBuilder.DropColumn(
                name: "SubstituidoPorId",
                table: "RefreshToken");

            migrationBuilder.DropColumn(
                name: "UserAgent",
                table: "RefreshToken");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "IpCriacao",
                table: "RefreshToken",
                type: "character varying(64)",
                maxLength: 64,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "MotivoRevogacao",
                table: "RefreshToken",
                type: "character varying(40)",
                maxLength: 40,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "SessaoExpiraEm",
                table: "RefreshToken",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<Guid>(
                name: "SessaoId",
                table: "RefreshToken",
                type: "uuid",
                nullable: false,
                defaultValue: Guid.Empty);

            migrationBuilder.AddColumn<Guid>(
                name: "SubstituidoPorId",
                table: "RefreshToken",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "UserAgent",
                table: "RefreshToken",
                type: "character varying(256)",
                maxLength: 256,
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_RefreshToken_SessaoId",
                table: "RefreshToken",
                column: "SessaoId");
        }
    }
}
