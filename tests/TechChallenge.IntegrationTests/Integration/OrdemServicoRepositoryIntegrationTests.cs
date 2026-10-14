using FluentAssertions;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using TechChallenge.Domain.Entities;
using TechChallenge.Domain.Enums;
using TechChallenge.Infrastructure.Database.Context;
using TechChallenge.Infrastructure.Database.Repositories;

namespace TechChallenge.IntegrationTests.Integration;

public class OrdemServicoRepositoryIntegrationTests
{
    [Fact]
    public async Task DeveOrdenarFilaOperacionalPorPrioridadeEAntiguidade()
    {
        await using var connection = new SqliteConnection("DataSource=:memory:");
        await connection.OpenAsync();
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseSqlite(connection)
            .Options;
        await using var context = new ApplicationDbContext(options);
        await context.Database.EnsureCreatedAsync();

        var endereco = new Endereco(Guid.NewGuid(), "Rua", "Casa", "1", "Centro", "Sao Paulo", "SP", "01001000");
        var cliente = new Cliente(Guid.NewGuid(), "Cliente", "cliente@teste.local", TipoDocumento.Cpf, "529.982.247-25", endereco.Id);
        cliente.AtribuirEndereco(endereco);
        var funcionario = new Funcionario(Guid.NewGuid(), "Mecanico", "529.982.247-25", "123456789", TipoFuncionario.Mecanico, endereco.Id);
        funcionario.AtribuirEndereco(endereco);
        var categoria = new CategoriaVeiculo(Guid.NewGuid(), "Passeio");
        var veiculo = new Veiculo(Guid.NewGuid(), "ABC1D23", "Modelo", "Marca", "Cor", 2020, 0, 0, cliente.Id, categoria.Id);

        context.AddRange(endereco, cliente, funcionario, categoria, veiculo);
        await context.SaveChangesAsync();

        var agora = DateTime.UtcNow;
        var recebidaAntiga = CriarOS(StatusOS.Recebida, agora.AddMinutes(-20));
        var recebidaNova = CriarOS(StatusOS.Recebida, agora.AddMinutes(-10));
        var diagnostico = CriarOS(StatusOS.EmDiagnostico, agora.AddMinutes(-5));
        var aguardando = CriarOS(StatusOS.AguardandoAprovacao, agora.AddMinutes(-4));
        var execucao = CriarOS(StatusOS.EmExecucao, agora.AddMinutes(-3));
        var reprovada = CriarOS(StatusOS.Reprovada, agora.AddMinutes(-30));
        var cancelada = CriarOS(StatusOS.Cancelada, agora.AddMinutes(-30));
        var finalizada = CriarOS(StatusOS.Finalizada, agora.AddMinutes(-30));

        context.OrdemServico.AddRange(
            recebidaNova, cancelada, diagnostico, finalizada,
            recebidaAntiga, execucao, reprovada, aguardando);
        await context.SaveChangesAsync();

        var repository = new OrdemServicoRepository(context);

        var resultado = await repository.GetAllAsync();

        resultado.Select(os => os.Id).Should().Equal(
            execucao.Id,
            aguardando.Id,
            diagnostico.Id,
            recebidaAntiga.Id,
            recebidaNova.Id);
        resultado.Should().NotContain(os =>
            os.Status == StatusOS.Reprovada ||
            os.Status == StatusOS.Cancelada ||
            os.Status == StatusOS.Finalizada ||
            os.Status == StatusOS.Entregue);

        OrdemServico CriarOS(StatusOS status, DateTime dataCriacao) => new(
            Guid.NewGuid(),
            $"OS {status}",
            Guid.NewGuid().ToString("N"),
            status,
            cliente.Id,
            funcionario.Id,
            veiculo.Id,
            dataCriacao,
            null,
            null,
            0,
            0,
            0);
    }
}
