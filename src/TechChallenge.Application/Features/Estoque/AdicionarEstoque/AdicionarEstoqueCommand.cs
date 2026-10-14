namespace TechChallenge.Application.Features.Estoque.AdicionarEstoque;

public class AdicionarEstoqueCommand
{
    public Guid ProdutoId { get; set; }
    public int Quantidade { get; set; }
}
