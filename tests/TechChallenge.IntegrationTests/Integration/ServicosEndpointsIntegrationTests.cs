using System;
using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using TechChallenge.Api.Models.Request;
using TechChallenge.Api.Models.Response;
using TechChallenge.IntegrationTests.Integration.Factories;

namespace TechChallenge.IntegrationTests.Integration;

public class ServicosEndpointsIntegrationTests : IClassFixture<WebAplicationFactory<Program>>
{
    private readonly HttpClient _client;

    public ServicosEndpointsIntegrationTests(WebAplicationFactory<Program> factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task DeveExecutarCrudDeServico()
    {
        var categoriaId = await CriarCategoriaServicoAsync();
        var criarRequest = new CriarServicoRequest
        {
            Descricao = "Troca de oleo",
            Valor = 120,
            CategoriaId = categoriaId
        };

        var criarResponse = await _client.PostAsJsonAsync("/api/v1/servicos", criarRequest);

        var criarBody = await criarResponse.Content.ReadAsStringAsync();
        criarResponse.StatusCode.Should().Be(HttpStatusCode.Created, criarBody);
        var servicoId = await criarResponse.Content.ReadFromJsonAsync<Guid>();
        servicoId.Should().NotBeEmpty();

        var obterResponse = await _client.GetAsync($"/api/v1/servicos/{servicoId}");
        obterResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var servico = await obterResponse.Content.ReadFromJsonAsync<ServicoResponse>();
        servico.Should().NotBeNull();
        servico.Descricao.Should().Be(criarRequest.Descricao);
        servico.Valor.Should().Be(criarRequest.Valor);

        var listarResponse = await _client.GetAsync("/api/v1/servicos");
        listarResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var servicos = await listarResponse.Content.ReadFromJsonAsync<List<ServicoResponse>>();
        servicos.Should().Contain(item => item.Id == servicoId);

        var atualizarRequest = new AtualizarServicoRequest
        {
            Descricao = "Troca de oleo premium",
            Valor = 180
        };
        var atualizarResponse = await _client.PutAsJsonAsync($"/api/v1/servicos/{servicoId}", atualizarRequest);
        atualizarResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var servicoAtualizado = await _client.GetFromJsonAsync<ServicoResponse>($"/api/v1/servicos/{servicoId}");
        servicoAtualizado.Should().NotBeNull();
        servicoAtualizado.Descricao.Should().Be(atualizarRequest.Descricao);
        servicoAtualizado.Valor.Should().Be(atualizarRequest.Valor);

        var excluirResponse = await _client.DeleteAsync($"/api/v1/servicos/{servicoId}");
        excluirResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);

        var obterExcluidoResponse = await _client.GetAsync($"/api/v1/servicos/{servicoId}");
        obterExcluidoResponse.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task DeveRetornarBadRequestQuandoServicoForInvalido()
    {
        var request = new CriarServicoRequest
        {
            Descricao = string.Empty,
            Valor = -1
        };

        var response = await _client.PostAsJsonAsync("/api/v1/servicos", request);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    private async Task<Guid> CriarCategoriaServicoAsync()
    {
        var response = await _client.PostAsJsonAsync("/api/v1/categoriaservico", new CriarCategoriaServicoRequest
        {
            Descricao = $"Categoria {Guid.NewGuid():N}"
        });

        var body = await response.Content.ReadAsStringAsync();
        response.StatusCode.Should().Be(HttpStatusCode.Created, body);
        return await response.Content.ReadFromJsonAsync<Guid>();
    }
}
