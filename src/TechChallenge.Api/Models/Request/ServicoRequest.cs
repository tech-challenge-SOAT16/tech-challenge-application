namespace TechChallenge.Api.Models.Request;

public class CriarServicoRequest
{
    public string Descricao { get; set; } = string.Empty;
    public decimal Valor { get; set; }
    public Guid CategoriaId { get; set; }
}

public class AtualizarServicoRequest
{
    public string Descricao { get; set; } = string.Empty;
    public decimal Valor { get; set; }
}
