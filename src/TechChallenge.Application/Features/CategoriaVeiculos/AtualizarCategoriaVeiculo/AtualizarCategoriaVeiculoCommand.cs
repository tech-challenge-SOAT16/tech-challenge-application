namespace TechChallenge.Application.Features.CategoriaVeiculos.AtualizarCategoriaVeiculo;

public class AtualizarCategoriaVeiculoCommand
{
    public Guid Id { get; set; }
    public string Descricao { get; set; } = string.Empty;
}