using System;
using TechChallenge.Domain.Enums;

namespace TechChallenge.Api.Models.Response;

public class ClienteResponse
{
    public Guid Id { get; set; }
    public string Nome { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public TipoDocumento TipoDocumento { get; set; }
    public string Documento { get; set; } = string.Empty;
    public EnderecoResponse? Endereco { get; set; }
}
