using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using TechChallenge.Api.Models.Request;
using TechChallenge.Api.Models.Response;
using TechChallenge.IntegrationTests.Integration.Factories;

namespace TechChallenge.IntegrationTests.Integration;

public class EstoqueEndpointsIntegrationTests : IClassFixture<WebAplicationFactory<Program>>
{
    private readonly HttpClient _client;

    public EstoqueEndpointsIntegrationTests(WebAplicationFactory<Program> factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task DeveConsultarAdicionarEBaixarEstoquePeloProduto()
    {
        var produtoId = await CriarProdutoAsync(10);

        var adicionarResponse = await _client.PostAsJsonAsync("/api/v1/estoque", new AdicionarEstoqueRequest
        {
            ProdutoId = produtoId,
            Quantidade = 5
        });

        var adicionarBody = await adicionarResponse.Content.ReadAsStringAsync();
        adicionarResponse.StatusCode.Should().Be(HttpStatusCode.Created, adicionarBody);
        var adicionado = await adicionarResponse.Content.ReadFromJsonAsync<EstoqueResponse>();
        adicionado.Should().NotBeNull();
        adicionado.Quantidade.Should().Be(15);

        var consultaResponse = await _client.GetAsync($"/api/v1/estoque/{produtoId}");
        consultaResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var consultado = await consultaResponse.Content.ReadFromJsonAsync<EstoqueResponse>();
        consultado.Should().NotBeNull();
        consultado.Quantidade.Should().Be(15);

        var baixaResponse = await _client.PutAsJsonAsync("/api/v1/estoque", new BaixarEstoqueRequest
        {
            ProdutoId = produtoId,
            Quantidade = 4
        });

        var baixaBody = await baixaResponse.Content.ReadAsStringAsync();
        baixaResponse.StatusCode.Should().Be(HttpStatusCode.OK, baixaBody);
        var baixado = await baixaResponse.Content.ReadFromJsonAsync<EstoqueResponse>();
        baixado.Should().NotBeNull();
        baixado.Quantidade.Should().Be(11);
    }

    [Fact]
    public async Task DeveImpedirSaldoNegativo()
    {
        var produtoId = await CriarProdutoAsync(2);

        var response = await _client.PutAsJsonAsync("/api/v1/estoque", new BaixarEstoqueRequest
        {
            ProdutoId = produtoId,
            Quantidade = 3
        });

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var estoque = await _client.GetFromJsonAsync<EstoqueResponse>($"/api/v1/estoque/{produtoId}");
        estoque.Should().NotBeNull();
        estoque.Quantidade.Should().Be(2);
    }

    [Fact]
    public async Task DeveRetornarNotFoundParaProdutoSemEstoque()
    {
        var response = await _client.GetAsync($"/api/v1/estoque/{Guid.NewGuid()}");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    private async Task<Guid> CriarProdutoAsync(int quantidade)
    {
        var categoriaResponse = await _client.PostAsJsonAsync("/api/v1/categoriaproduto", new CriarCategoriaProdutoRequest
        {
            Descricao = $"Categoria {Guid.NewGuid():N}"
        });
        var categoriaBody = await categoriaResponse.Content.ReadAsStringAsync();
        categoriaResponse.StatusCode.Should().Be(HttpStatusCode.Created, categoriaBody);
        var categoriaId = await categoriaResponse.Content.ReadFromJsonAsync<Guid>();

        var produtoResponse = await _client.PostAsJsonAsync("/api/v1/produtos", new CriarProdutoRequest
        {
            Descricao = $"Produto {Guid.NewGuid():N}",
            Valor = 10,
            Quantidade = quantidade,
            IdCategoria = categoriaId
        });
        var produtoBody = await produtoResponse.Content.ReadAsStringAsync();
        produtoResponse.StatusCode.Should().Be(HttpStatusCode.Created, produtoBody);
        return await produtoResponse.Content.ReadFromJsonAsync<Guid>();
    }
}
