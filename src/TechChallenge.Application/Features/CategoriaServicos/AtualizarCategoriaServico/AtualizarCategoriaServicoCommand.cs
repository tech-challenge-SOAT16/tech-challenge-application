namespace TechChallenge.Application.Features.CategoriaServicos.AtualizarCategoriaServico;

public class AtualizarCategoriaServicoCommand
{
    public Guid Id { get; set; }
    public string Descricao { get; set; } = string.Empty;
}