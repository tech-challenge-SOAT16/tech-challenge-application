namespace TechChallenge.Api.Models.Request;

using TechChallenge.Domain.Enums;

public class CriarClienteRequest
{
    public string? Nome { get; set; }
    public string? Email { get; set; }
    public TipoDocumento TipoDocumento { get; set; }
    public string? Documento { get; set; }
    public EnderecoRequest? Endereco { get; set; }
}

public class AtualizarClienteRequest
{
    public string? Nome { get; set; }
    public string? Email { get; set; }
    public TipoDocumento TipoDocumento { get; set; }
    public string? Documento { get; set; }
    public EnderecoRequest? Endereco { get; set; }
}
