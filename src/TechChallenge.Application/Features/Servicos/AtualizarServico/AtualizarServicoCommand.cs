using System;
namespace TechChallenge.Application.Features.Servicos.AtualizarServico;

public class AtualizarServicoCommand
{
    public Guid Id { get; set; }
    public string Descricao { get; set; } = string.Empty;
    public decimal Valor { get; set; }
    public Guid CategoriaId { get; set; }
}
