using System;
namespace TechChallenge.Application.Features.Inventario.AtualizarItemInventario;

public class AtualizarItemInventarioCommand
{
    public Guid Id { get; set; }
    public string Descricao { get; set; } = string.Empty;
    public decimal Valor { get; set; }
    public int Quantidade { get; set; }
}
