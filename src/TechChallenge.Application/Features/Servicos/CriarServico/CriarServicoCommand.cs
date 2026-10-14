namespace TechChallenge.Application.Features.Servicos.CriarServico;

public class CriarServicoCommand
{
    public string Descricao { get; set; } = string.Empty;
    public decimal Valor { get; set; }
    public Guid CategoriaId { get; set; }
}
