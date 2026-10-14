namespace TechChallenge.Application.Features.Estoque.BaixarEstoque;

public class BaixarEstoqueCommand
{
    public Guid ProdutoId { get; set; }
    public int Quantidade { get; set; }
}
