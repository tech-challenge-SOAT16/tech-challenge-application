using FluentAssertions;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using TechChallenge.Domain.Entities;
using TechChallenge.Infrastructure.Database.Context;

namespace TechChallenge.IntegrationTests.Integration;

public class EstoqueConcurrencyIntegrationTests
{
    [Fact]
    public async Task DeveRejeitarAtualizacaoComVersaoDesatualizada()
    {
        await using var connection = new SqliteConnection("DataSource=:memory:");
        await connection.OpenAsync();
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseSqlite(connection)
            .Options;

        await using (var setup = new ApplicationDbContext(options))
        {
            await setup.Database.EnsureCreatedAsync();
            var categoria = new CategoriaProduto(Guid.NewGuid(), "Filtros");
            var produto = new Produto(Guid.NewGuid(), "Filtro", 50, categoria.Id);
            var estoque = new Estoque(Guid.NewGuid(), produto.Id, 10);
            setup.AddRange(categoria, produto, estoque);
            await setup.SaveChangesAsync();
        }

        await using var primeiroContexto = new ApplicationDbContext(options);
        await using var segundoContexto = new ApplicationDbContext(options);
        var primeiro = await primeiroContexto.Estoque.SingleAsync();
        var segundo = await segundoContexto.Estoque.SingleAsync();

        primeiro.Adicionar(1);
        await primeiroContexto.SaveChangesAsync();

        segundo.Adicionar(1);
        var act = () => segundoContexto.SaveChangesAsync();

        await act.Should().ThrowAsync<DbUpdateConcurrencyException>();
    }
}
