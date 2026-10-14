using System;
using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using TechChallenge.Api.Models.Request;
using TechChallenge.Api.Models.Response;
using TechChallenge.Domain.Enums;
using TechChallenge.IntegrationTests.Integration.Factories;

namespace TechChallenge.IntegrationTests.Integration;

public class FuncionariosEndpointsIntegrationTests : IClassFixture<WebAplicationFactory<Program>>
{
    private readonly HttpClient _client;

    public FuncionariosEndpointsIntegrationTests(WebAplicationFactory<Program> factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task DeveExecutarCrudDeFuncionario()
    {
        var criarRequest = CriarFuncionarioRequest("52998224725", TipoFuncionario.Mecanico);

        var criarResponse = await _client.PostAsJsonAsync("/api/v1/funcionarios", criarRequest);

        var criarBody = await criarResponse.Content.ReadAsStringAsync();
        criarResponse.StatusCode.Should().Be(HttpStatusCode.Created, criarBody);
        var funcionarioId = await criarResponse.Content.ReadFromJsonAsync<Guid>();
        funcionarioId.Should().NotBeEmpty();

        var obterResponse = await _client.GetAsync($"/api/v1/funcionarios/{funcionarioId}");
        obterResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var funcionario = await obterResponse.Content.ReadFromJsonAsync<FuncionarioResponse>();
        funcionario.Should().NotBeNull();
        funcionario.Nome.Should().Be(criarRequest.Nome);
        funcionario.Cpf.Should().Be("529.982.247-25");
        funcionario.Cargo.Should().Be(criarRequest.Cargo.ToString());
        funcionario.Endereco.Should().NotBeNull();
        funcionario.Endereco.Cidade.Should().Be(criarRequest.Endereco!.Cidade);

        var listarResponse = await _client.GetAsync("/api/v1/funcionarios");
        listarResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var funcionarios = await listarResponse.Content.ReadFromJsonAsync<List<FuncionarioResponse>>();
        funcionarios.Should().Contain(f => f.Id == funcionarioId);

        var atualizarRequest = AtualizarFuncionarioRequest("39053344705", "Administrador");
        var atualizarResponse = await _client.PutAsJsonAsync($"/api/v1/funcionarios/{funcionarioId}", atualizarRequest);
        atualizarResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var funcionarioAtualizado = await _client.GetFromJsonAsync<FuncionarioResponse>($"/api/v1/funcionarios/{funcionarioId}");
        funcionarioAtualizado.Should().NotBeNull();
        funcionarioAtualizado.Nome.Should().Be(atualizarRequest.Nome);
        funcionarioAtualizado.Cpf.Should().Be("390.533.447-05");
        funcionarioAtualizado.Cargo.Should().Be(atualizarRequest.Cargo);
        funcionarioAtualizado.Endereco!.Cidade.Should().Be(atualizarRequest.Endereco!.Cidade);

        var excluirResponse = await _client.DeleteAsync($"/api/v1/funcionarios/{funcionarioId}");
        excluirResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);

        var obterExcluidoResponse = await _client.GetAsync($"/api/v1/funcionarios/{funcionarioId}");
        obterExcluidoResponse.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task DeveRetornarBadRequestQuandoFuncionarioInvalido()
    {
        var request = new
        {
            Nome = "Joao Funcionario",
            Cpf = "12345678901",
            Rg = "123456789",
            Cargo = "CargoInvalido",
            Endereco = CriarEnderecoRequest("Sao Paulo")
        };

        var response = await _client.PostAsJsonAsync("/api/v1/funcionarios", request);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    private static CriarFuncionarioRequest CriarFuncionarioRequest(string cpf, TipoFuncionario cargo)
    {
        return new CriarFuncionarioRequest
        {
            Nome = "Joao Funcionario",
            Cpf = cpf,
            Rg = "123456789",
            Cargo = cargo,
            Endereco = CriarEnderecoRequest("Sao Paulo")
        };
    }

    private static AtualizarFuncionarioRequest AtualizarFuncionarioRequest(string cpf, string cargo)
    {
        return new AtualizarFuncionarioRequest
        {
            Nome = "Joao Funcionario Atualizado",
            Cpf = cpf,
            Rg = "987654321",
            Cargo = cargo,
            Endereco = CriarEnderecoRequest("Curitiba")
        };
    }

    private static EnderecoRequest CriarEnderecoRequest(string cidade)
    {
        return new EnderecoRequest
        {
            Logradouro = "Rua Teste",
            Complemento = "Casa",
            Numero = "200",
            Bairro = "Centro",
            Cidade = cidade,
            Estado = "SP",
            Cep = "01001000"
        };
    }
}
