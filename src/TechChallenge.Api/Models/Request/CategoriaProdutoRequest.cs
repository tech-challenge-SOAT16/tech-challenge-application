namespace TechChallenge.Api.Models.Request
{
    public class CriarCategoriaProdutoRequest
    {
        public string? Descricao { get; set; }
    }

    public class ConsultarCategoriaProdutoRequest
    {
        public Guid Id { get; set; }
    }

    public class AtualizarCategoriaProdutoRequest
    {
        public Guid Id { get; set; }
        public string? Descricao { get; set; }
    }

    public class ExcluirCategoriaProdutoRequest
    {
        public Guid Id { get; set; }
    }
}