using TechChallenge.Domain.Enums;

namespace TechChallenge.Api.Models.Request
{
    public class CriarCategoriaServicoRequest
    {
        public string? Descricao { get; set; }
    }

    public class ConsultarCategoriaServicoRequest
    {
        public Guid Id { get; set; }
    }

    public class AtualizarCategoriaServicoRequest
    {
        public Guid Id { get; set; }
        public string? Descricao { get; set; }
    }

    public class ExcluirCategoriaServicoRequest
    {
        public Guid Id { get; set; }
    }
}