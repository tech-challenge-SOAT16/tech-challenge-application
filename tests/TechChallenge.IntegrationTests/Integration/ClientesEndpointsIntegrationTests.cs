using System;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using FluentAssertions;
using TechChallenge.Api.Models.Request;
using TechChallenge.Api.Models.Response;
using TechChallenge.Domain.Enums;
using TechChallenge.IntegrationTests.Integration.Factories;

namespace TechChallenge.IntegrationTests.Integration;

public class ClientesEndpointsIntegrationTests : IClassFixture<WebAplicationFactory<Program>>
{
    private readonly HttpClient _client;

    public ClientesEndpointsIntegrationTests(WebAplicationFactory<Program> factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task DeveExecutarCrudDeCliente()
    {
        var criarRequest = CriarClienteRequest("52998224725");

        var criarResponse = await _client.PostAsJsonAsync("/api/v1/clientes", criarRequest);

        var criarBody = await criarResponse.Content.ReadAsStringAsync();
        criarResponse.StatusCode.Should().Be(HttpStatusCode.Created, criarBody);
        var clienteId = await criarResponse.Content.ReadFromJsonAsync<Guid>();
        clienteId.Should().NotBeEmpty();

        var obterResponse = await _client.GetAsync($"/api/v1/clientes/{clienteId}");
        obterResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var cliente = await obterResponse.Content.ReadFromJsonAsync<ClienteResponse>(JsonTestOptions.Web);
        cliente.Should().NotBeNull();
        cliente.Nome.Should().Be(criarRequest.Nome);
        cliente.Email.Should().Be(criarRequest.Email);
        cliente.TipoDocumento.Should().Be(TipoDocumento.Cpf);
        cliente.Documento.Should().Be("529.982.247-25");
        cliente.Endereco.Should().NotBeNull();
        cliente.Endereco.Cidade.Should().Be(criarRequest.Endereco!.Cidade);

        var listarResponse = await _client.GetAsync("/api/v1/clientes");
        listarResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var clientes = await listarResponse.Content.ReadFromJsonAsync<List<ClienteResponse>>(JsonTestOptions.Web);
        clientes.Should().Contain(c => c.Id == clienteId);

        var atualizarRequest = AtualizarClienteRequest("39053344705");
        var atualizarResponse = await _client.PutAsJsonAsync($"/api/v1/clientes/{clienteId}", atualizarRequest);
        atualizarResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var clienteAtualizado = await _client.GetFromJsonAsync<ClienteResponse>(
            $"/api/v1/clientes/{clienteId}",
            JsonTestOptions.Web);
        clienteAtualizado.Should().NotBeNull();
        clienteAtualizado.Nome.Should().Be(atualizarRequest.Nome);
        clienteAtualizado.Email.Should().Be(atualizarRequest.Email);
        clienteAtualizado.TipoDocumento.Should().Be(TipoDocumento.Cpf);
        clienteAtualizado.Documento.Should().Be("390.533.447-05");
        clienteAtualizado.Endereco!.Cidade.Should().Be(atualizarRequest.Endereco!.Cidade);

        var excluirResponse = await _client.DeleteAsync($"/api/v1/clientes/{clienteId}");
        excluirResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);

        var obterExcluidoResponse = await _client.GetAsync($"/api/v1/clientes/{clienteId}");
        obterExcluidoResponse.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task DeveRetornarBadRequestQuandoClienteInvalido()
    {
        var request = CriarClienteRequest("12345678901");
        request.Nome = string.Empty;

        var response = await _client.PostAsJsonAsync("/api/v1/clientes", request);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task DeveRetornarBadRequestQuandoRgEEnderecoNaoForemInformados()
    {
        var request = new CriarClienteRequest
        {
            Nome = "Maria Cliente",
            TipoDocumento = TipoDocumento.Cpf,
            Documento = "52998224725"
        };

        var response = await _client.PostAsJsonAsync("/api/v1/clientes", request);
        var body = await response.Content.ReadAsStringAsync();

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest, body);
    }

    [Fact]
    public async Task DeveAceitarTipoDocumentoComoTexto()
    {
        var request = new
        {
            Nome = "Oficina Cliente LTDA",
            Email = "oficina.cliente@teste.local",
            TipoDocumento = "Cnpj",
            Documento = "11222333000181",
            Endereco = CriarEnderecoRequest("Sao Paulo")
        };

        var criarResponse = await _client.PostAsJsonAsync("/api/v1/clientes", request);
        var criarBody = await criarResponse.Content.ReadAsStringAsync();

        criarResponse.StatusCode.Should().Be(HttpStatusCode.Created, criarBody);
        var clienteId = await criarResponse.Content.ReadFromJsonAsync<Guid>();

        var cliente = await _client.GetFromJsonAsync<ClienteResponse>(
            $"/api/v1/clientes/{clienteId}",
            JsonTestOptions.Web);

        cliente.Should().NotBeNull();
        cliente.TipoDocumento.Should().Be(TipoDocumento.Cnpj);
        cliente.Documento.Should().Be("11.222.333/0001-81");
    }

    [Fact]
    public async Task DeveRetornarProblemDetailsSemDetalhesInternosQuandoClienteJaExiste()
    {
        var request = CriarClienteRequest("121.344.187-02");

        var criarResponse = await _client.PostAsJsonAsync("/api/v1/clientes", request);
        criarResponse.StatusCode.Should().Be(HttpStatusCode.Created);

        request.Documento = "121344187-02";
        var response = await _client.PostAsJsonAsync("/api/v1/clientes", request);
        var body = await response.Content.ReadAsStringAsync();

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest, body);

        using var json = JsonDocument.Parse(body);
        var root = json.RootElement;

        root.GetProperty("status").GetInt32().Should().Be((int)HttpStatusCode.BadRequest);
        root.GetProperty("title").GetString().Should().Be("Requisição inválida.");
        root.GetProperty("detail").GetString().Should().Be("Já existe um cliente cadastrado com o CPF 121.344.187-02.");
        root.TryGetProperty("traceId", out _).Should().BeTrue();
        root.TryGetProperty("details", out _).Should().BeFalse();
        root.TryGetProperty("stackTrace", out _).Should().BeFalse();
    }

    private static CriarClienteRequest CriarClienteRequest(string cpf)
    {
        return new CriarClienteRequest
        {
            Nome = "Maria Cliente",
            Email = "maria.cliente@teste.local",
            TipoDocumento = TipoDocumento.Cpf,
            Documento = cpf,
            Endereco = CriarEnderecoRequest("Sao Paulo")
        };
    }

    private static AtualizarClienteRequest AtualizarClienteRequest(string cpf)
    {
        return new AtualizarClienteRequest
        {
            Nome = "Maria Cliente Atualizada",
            Email = "maria.atualizada@teste.local",
            TipoDocumento = TipoDocumento.Cpf,
            Documento = cpf,
            Endereco = CriarEnderecoRequest("Curitiba")
        };
    }

    private static EnderecoRequest CriarEnderecoRequest(string cidade)
    {
        return new EnderecoRequest
        {
            Logradouro = "Rua Teste",
            Complemento = "Apto 10",
            Numero = "100",
            Bairro = "Centro",
            Cidade = cidade,
            Estado = "SP",
            Cep = "01001000"
        };
    }
}
