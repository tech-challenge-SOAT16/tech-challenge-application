using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TechChallenge.Infrastructure.Database.Migrations
{
    /// <inheritdoc />
    public partial class AlignRequiredRelationshipsAndIndexes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Cliente_Endereco_EnderecoId",
                table: "Cliente");

            migrationBuilder.DropForeignKey(
                name: "FK_Funcionario_Endereco_EnderecoId",
                table: "Funcionario");

            migrationBuilder.DropForeignKey(
                name: "FK_OrdemServico_Cliente_ClienteResponsavelId",
                table: "OrdemServico");

            migrationBuilder.DropForeignKey(
                name: "FK_OrdemServico_Funcionario_FuncionarioResponsavelId",
                table: "OrdemServico");

            migrationBuilder.DropForeignKey(
                name: "FK_OrdemServico_Veiculo_VeiculoId",
                table: "OrdemServico");

            migrationBuilder.DropForeignKey(
                name: "FK_OrdemServicoProdutos_Produto_ProdutoId",
                table: "OrdemServicoProdutos");

            migrationBuilder.DropForeignKey(
                name: "FK_OrdemServicoServicos_Servico_ServicoId",
                table: "OrdemServicoServicos");

            migrationBuilder.Sql("""
                DO $$
                BEGIN
                    IF EXISTS (SELECT 1 FROM "Veiculo" GROUP BY "Placa" HAVING COUNT(*) > 1) THEN
                        RAISE EXCEPTION 'Não é possível criar índice único: existem veículos com placa duplicada.';
                    END IF;

                    IF EXISTS (SELECT 1 FROM "Funcionario" GROUP BY "Cpf" HAVING COUNT(*) > 1) THEN
                        RAISE EXCEPTION 'Não é possível criar índice único: existem funcionários com CPF duplicado.';
                    END IF;

                    IF EXISTS (SELECT 1 FROM "Cliente" GROUP BY "Cpf" HAVING COUNT(*) > 1) THEN
                        RAISE EXCEPTION 'Não é possível criar índice único: existem clientes com CPF duplicado.';
                    END IF;
                END $$;
                """);

            migrationBuilder.CreateIndex(
                name: "IX_Veiculo_Placa",
                table: "Veiculo",
                column: "Placa",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Funcionario_Cpf",
                table: "Funcionario",
                column: "Cpf",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Cliente_Cpf",
                table: "Cliente",
                column: "Cpf",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Cliente_Endereco_EnderecoId",
                table: "Cliente",
                column: "EnderecoId",
                principalTable: "Endereco",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Funcionario_Endereco_EnderecoId",
                table: "Funcionario",
                column: "EnderecoId",
                principalTable: "Endereco",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_OrdemServico_Cliente_ClienteResponsavelId",
                table: "OrdemServico",
                column: "ClienteResponsavelId",
                principalTable: "Cliente",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_OrdemServico_Funcionario_FuncionarioResponsavelId",
                table: "OrdemServico",
                column: "FuncionarioResponsavelId",
                principalTable: "Funcionario",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_OrdemServico_Veiculo_VeiculoId",
                table: "OrdemServico",
                column: "VeiculoId",
                principalTable: "Veiculo",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_OrdemServicoProdutos_Produto_ProdutoId",
                table: "OrdemServicoProdutos",
                column: "ProdutoId",
                principalTable: "Produto",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_OrdemServicoServicos_Servico_ServicoId",
                table: "OrdemServicoServicos",
                column: "ServicoId",
                principalTable: "Servico",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Cliente_Endereco_EnderecoId",
                table: "Cliente");

            migrationBuilder.DropForeignKey(
                name: "FK_Funcionario_Endereco_EnderecoId",
                table: "Funcionario");

            migrationBuilder.DropForeignKey(
                name: "FK_OrdemServico_Cliente_ClienteResponsavelId",
                table: "OrdemServico");

            migrationBuilder.DropForeignKey(
                name: "FK_OrdemServico_Funcionario_FuncionarioResponsavelId",
                table: "OrdemServico");

            migrationBuilder.DropForeignKey(
                name: "FK_OrdemServico_Veiculo_VeiculoId",
                table: "OrdemServico");

            migrationBuilder.DropForeignKey(
                name: "FK_OrdemServicoProdutos_Produto_ProdutoId",
                table: "OrdemServicoProdutos");

            migrationBuilder.DropForeignKey(
                name: "FK_OrdemServicoServicos_Servico_ServicoId",
                table: "OrdemServicoServicos");

            migrationBuilder.DropIndex(
                name: "IX_Veiculo_Placa",
                table: "Veiculo");

            migrationBuilder.DropIndex(
                name: "IX_Funcionario_Cpf",
                table: "Funcionario");

            migrationBuilder.DropIndex(
                name: "IX_Cliente_Cpf",
                table: "Cliente");

            migrationBuilder.AddForeignKey(
                name: "FK_Cliente_Endereco_EnderecoId",
                table: "Cliente",
                column: "EnderecoId",
                principalTable: "Endereco",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Funcionario_Endereco_EnderecoId",
                table: "Funcionario",
                column: "EnderecoId",
                principalTable: "Endereco",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_OrdemServico_Cliente_ClienteResponsavelId",
                table: "OrdemServico",
                column: "ClienteResponsavelId",
                principalTable: "Cliente",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_OrdemServico_Funcionario_FuncionarioResponsavelId",
                table: "OrdemServico",
                column: "FuncionarioResponsavelId",
                principalTable: "Funcionario",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_OrdemServico_Veiculo_VeiculoId",
                table: "OrdemServico",
                column: "VeiculoId",
                principalTable: "Veiculo",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_OrdemServicoProdutos_Produto_ProdutoId",
                table: "OrdemServicoProdutos",
                column: "ProdutoId",
                principalTable: "Produto",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_OrdemServicoServicos_Servico_ServicoId",
                table: "OrdemServicoServicos",
                column: "ServicoId",
                principalTable: "Servico",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
