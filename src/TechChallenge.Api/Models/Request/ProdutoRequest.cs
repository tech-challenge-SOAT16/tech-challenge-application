namespace TechChallenge.Api.Models.Request;

public class CriarProdutoRequest
{
    public string Descricao { get; set; } = string.Empty;
    public decimal Valor { get; set; }
    public int Quantidade { get; set; }
    public Guid IdCategoria { get; set; }
}

public class AtualizarProdutoRequest
{
    public string Descricao { get; set; } = string.Empty;
    public decimal Valor { get; set; }
    public int Quantidade { get; set; }
}
