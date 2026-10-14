namespace TechChallenge.Api.Models.Request
{
    public class AdicionarEstoqueRequest
    {
        public Guid ProdutoId { get; set; }
        public int Quantidade { get; set; }
    }

    public class BaixarEstoqueRequest
    {
        public Guid ProdutoId { get; set; }
        public int Quantidade { get; set; }
    }

    public class ObterEstoqueRequest
    {
        public Guid ProdutoId { get; set; }
    }
}