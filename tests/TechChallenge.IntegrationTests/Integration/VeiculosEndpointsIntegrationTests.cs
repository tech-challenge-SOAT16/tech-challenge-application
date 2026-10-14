using FluentAssertions;
using System.Net;
using System.Net.Http.Json;
using System.Text;
using TechChallenge.Api.Models.Request;
using TechChallenge.Api.Models.Response;
using TechChallenge.Domain.Enums;
using TechChallenge.IntegrationTests.Integration.Factories;

namespace TechChallenge.IntegrationTests.Integration;

public class VeiculosEndpointsIntegrationTests : IClassFixture<WebAplicationFactory<Program>>
{
    private static int _sequencia;
    private readonly HttpClient _client;

    public VeiculosEndpointsIntegrationTests(WebAplicationFactory<Program> factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task DeveExecutarCrudDeVeiculo()
    {
        var clienteId = await CriarClienteAsync();
        var categoriaId = await CriarCategoriaVeiculoAsync();
        var sequencia = Interlocked.Increment(ref _sequencia);
        var placa = $"CAR{sequencia % 10}A{sequencia % 10}{(sequencia + 1) % 10}";
        var criarRequest = new CriarVeiculoRequest
        {
            Placa = placa,
            Modelo = "Civic",
            Marca = "Honda",
            Cor = "Prata",
            Ano = 2022,
            Quilometragem = 10000,
            Valor = 90000,
            ClienteId = clienteId,
            CategoriaId = categoriaId
        };

        var criarResponse = await _client.PostAsJsonAsync("/api/v1/veiculos", criarRequest);

        var criarBody = await criarResponse.Content.ReadAsStringAsync();
        criarResponse.StatusCode.Should().Be(HttpStatusCode.Created, criarBody);
        var veiculoId = await criarResponse.Content.ReadFromJsonAsync<Guid>();
        veiculoId.Should().NotBeEmpty();

        var obterResponse = await _client.GetAsync($"/api/v1/veiculos/{veiculoId}");
        obterResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var veiculo = await obterResponse.Content.ReadFromJsonAsync<VeiculoResponse>(JsonTestOptions.Web);
        veiculo.Should().NotBeNull();
        veiculo.Placa.Should().Be(placa);
        veiculo.ClienteId.Should().Be(clienteId);

        var listarResponse = await _client.GetAsync("/api/v1/veiculos");
        listarResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var veiculos = await listarResponse.Content.ReadFromJsonAsync<List<VeiculoResponse>>(JsonTestOptions.Web);
        veiculos.Should().Contain(v => v.Id == veiculoId);

        var atualizarRequest = new AtualizarVeiculoRequest
        {
            Placa = $"MOT{sequencia % 10}B{sequencia % 10}{(sequencia + 2) % 10}",
            Modelo = "CG",
            Marca = "Honda",
            Cor = "Preta",
            Ano = 2023,
            Quilometragem = 5000,
            Valor = 18000,
            ClienteId = clienteId,
            CategoriaId = categoriaId
        };
        var atualizarResponse = await _client.PutAsJsonAsync($"/api/v1/veiculos/{veiculoId}", atualizarRequest);
        atualizarResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var atualizado = await _client.GetFromJsonAsync<VeiculoResponse>(
            $"/api/v1/veiculos/{veiculoId}",
            JsonTestOptions.Web);
        atualizado.Should().NotBeNull();
        atualizado.Placa.Should().Be(atualizarRequest.Placa);
        atualizado.Modelo.Should().Be(atualizarRequest.Modelo);
        atualizado.CategoriaId.Should().Be(categoriaId);

        var excluirResponse = await _client.DeleteAsync($"/api/v1/veiculos/{veiculoId}");
        excluirResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);

        var obterExcluidoResponse = await _client.GetAsync($"/api/v1/veiculos/{veiculoId}");
        obterExcluidoResponse.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task DeveRetornarBadRequestQuandoCriarVeiculoSemBody()
    {
        var response = await _client.PostAsync("/api/v1/veiculos", null);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task DeveRetornarBadRequestQuandoCriarVeiculoComBodyVazio()
    {
        using var content = new StringContent("{}", Encoding.UTF8, "application/json");

        var response = await _client.PostAsync("/api/v1/veiculos", content);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    private async Task<Guid> CriarClienteAsync()
    {
        var sequencia = Interlocked.Increment(ref _sequencia);
        var request = new CriarClienteRequest
        {
            Nome = "Cliente Veiculo",
            Email = $"cliente.veiculo.{sequencia}@teste.local",
            TipoDocumento = TipoDocumento.Cpf,
            Documento = GerarCpf(sequencia),
            Endereco = new EnderecoRequest
            {
                Logradouro = "Rua Teste",
                Complemento = "Casa",
                Numero = "100",
                Bairro = "Centro",
                Cidade = "Sao Paulo",
                Estado = "SP",
                Cep = "01001000"
            }
        };

        var response = await _client.PostAsJsonAsync("/api/v1/clientes", request);
        var body = await response.Content.ReadAsStringAsync();
        response.StatusCode.Should().Be(HttpStatusCode.Created, body);
        return await response.Content.ReadFromJsonAsync<Guid>();
    }

    private async Task<Guid> CriarCategoriaVeiculoAsync()
    {
        var sequencia = Interlocked.Increment(ref _sequencia);
        var request = new CriarCategoriaVeiculoRequest
        {
            Descricao = "Sedan",
        };

        var response = await _client.PostAsJsonAsync("/api/v1/categoriaveiculo", request);
        var body = await response.Content.ReadAsStringAsync();
        response.StatusCode.Should().Be(HttpStatusCode.Created, body);
        return await response.Content.ReadFromJsonAsync<Guid>();
    }

    private static string GerarCpf(int seed)
    {
        var digitos = new int[11];
        var valor = Math.Abs(seed) + 87654321;

        for (var i = 7; i >= 0; i--)
        {
            digitos[i] = valor % 10;
            valor /= 10;
        }

        digitos[8] = seed % 10;
        digitos[9] = CalcularDigito(digitos, 9);
        digitos[10] = CalcularDigito(digitos, 10);

        return string.Concat(digitos);
    }

    private static int CalcularDigito(int[] digitos, int tamanho)
    {
        var soma = 0;
        for (var i = 0; i < tamanho; i++)
            soma += digitos[i] * (tamanho + 1 - i);

        var resto = soma % 11;
        return resto < 2 ? 0 : 11 - resto;
    }
}
