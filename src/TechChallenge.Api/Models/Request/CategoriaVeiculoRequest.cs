namespace TechChallenge.Api.Models.Request
{
    public class CriarCategoriaVeiculoRequest
    {
        public string? Descricao { get; set; }
    }

    public class ConsultarCategoriaVeiculoRequest
    {
        public Guid Id { get; set; }
    }

    public class AtualizarCategoriaVeiculoRequest
    {
        public Guid Id { get; set; }
        public string? Descricao { get; set; }
    }

    public class ExcluirCategoriaVeiculoRequest
    {
        public Guid Id { get; set; }
    }
}