using System;
namespace TechChallenge.Api.Models.Response;

public class FuncionarioResponse
{
    public Guid Id { get; set; }
    public string Nome { get; set; } = string.Empty;
    public string Cpf { get; set; } = string.Empty;
    public string Rg { get; set; } = string.Empty;
    public string Cargo { get; set; } = string.Empty;
    public EnderecoResponse? Endereco { get; set; }
}
