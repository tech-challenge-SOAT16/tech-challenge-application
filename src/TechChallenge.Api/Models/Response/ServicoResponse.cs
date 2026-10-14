using System;
namespace TechChallenge.Api.Models.Response;

public class ServicoResponse
{
    public Guid Id { get; set; }
    public string Descricao { get; set; } = string.Empty;
    public decimal Valor { get; set; }
}
