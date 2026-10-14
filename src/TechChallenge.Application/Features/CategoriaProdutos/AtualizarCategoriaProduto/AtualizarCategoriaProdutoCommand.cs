namespace TechChallenge.Application.Features.CategoriaProdutos.AtualizarCategoriaProduto;

public class AtualizarCategoriaProdutoCommand
{
    public Guid Id { get; set; }
    public string Descricao { get; set; } = string.Empty;
}