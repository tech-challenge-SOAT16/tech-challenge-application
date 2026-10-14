using System;
namespace TechChallenge.Api.Models.Response;

public class ProdutoResponse
{
    public Guid Id { get; set; }
    public string Descricao { get; set; } = string.Empty;
    public decimal Valor { get; set; }
    public int Quantidade { get; set; }
    public Guid CategoriaId { get; set; }
}
