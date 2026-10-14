using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using TechChallenge.Api.Models.Request;
using TechChallenge.Api.Models.Response;
using TechChallenge.IntegrationTests.Integration.Factories;

namespace TechChallenge.IntegrationTests.Integration;

public class InventarioEndpointsIntegrationTests : IClassFixture<WebAplicationFactory<Program>>
{
    private readonly HttpClient _client;

    public InventarioEndpointsIntegrationTests(WebAplicationFactory<Program> factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task DeveExecutarCrudDeProduto()
    {
        var categoriaId = await CriarCategoriaProdutoAsync("Filtros");
        var criarRequest = new CriarProdutoRequest
        {
            Descricao = "Filtro de oleo",
            Valor = 45,
            Quantidade = 10,
            IdCategoria = categoriaId
        };

        var criarResponse = await _client.PostAsJsonAsync("/api/v1/produtos", criarRequest);

        var criarBody = await criarResponse.Content.ReadAsStringAsync();
        criarResponse.StatusCode.Should().Be(HttpStatusCode.Created, criarBody);
        var produtoId = await criarResponse.Content.ReadFromJsonAsync<Guid>();
        produtoId.Should().NotBeEmpty();

        var obterResponse = await _client.GetAsync($"/api/v1/produtos/{produtoId}");
        obterResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var produto = await obterResponse.Content.ReadFromJsonAsync<ProdutoResponse>();
        produto.Should().NotBeNull();
        produto.Descricao.Should().Be(criarRequest.Descricao);
        produto.Valor.Should().Be(criarRequest.Valor);
        produto.Quantidade.Should().Be(criarRequest.Quantidade);
        produto.CategoriaId.Should().Be(categoriaId);

        var listarResponse = await _client.GetAsync("/api/v1/produtos");
        listarResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var produtos = await listarResponse.Content.ReadFromJsonAsync<List<ProdutoResponse>>();
        produtos.Should().Contain(p => p.Id == produtoId);

        var atualizarRequest = new AtualizarProdutoRequest
        {
            Descricao = "Filtro de oleo premium",
            Valor = 60,
            Quantidade = 7
        };
        var atualizarResponse = await _client.PutAsJsonAsync($"/api/v1/produtos/{produtoId}", atualizarRequest);
        atualizarResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var atualizado = await _client.GetFromJsonAsync<ProdutoResponse>($"/api/v1/produtos/{produtoId}");
        atualizado.Should().NotBeNull();
        atualizado.Descricao.Should().Be(atualizarRequest.Descricao);
        atualizado.Valor.Should().Be(atualizarRequest.Valor);
        atualizado.Quantidade.Should().Be(atualizarRequest.Quantidade);

        var excluirResponse = await _client.DeleteAsync($"/api/v1/produtos/{produtoId}");
        excluirResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);

        var obterExcluidoResponse = await _client.GetAsync($"/api/v1/produtos/{produtoId}");
        obterExcluidoResponse.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task DeveRetornarBadRequestQuandoProdutoForInvalido()
    {
        var request = new CriarProdutoRequest
        {
            Descricao = string.Empty,
            Valor = -1
        };

        var response = await _client.PostAsJsonAsync("/api/v1/produtos", request);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    private async Task<Guid> CriarCategoriaProdutoAsync(string descricao)
    {
        var response = await _client.PostAsJsonAsync("/api/v1/categoriaproduto", new CriarCategoriaProdutoRequest
        {
            Descricao = descricao
        });

        var body = await response.Content.ReadAsStringAsync();
        response.StatusCode.Should().Be(HttpStatusCode.Created, body);
        return await response.Content.ReadFromJsonAsync<Guid>();
    }
}
