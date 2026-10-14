namespace TechChallenge.Application.Features.Inventario.CriarItemInventario;

public class CriarItemInventarioCommand
{
    public string Descricao { get; set; } = string.Empty;
    public decimal Valor { get; set; }
    public int Quantidade { get; set; }
    public Guid IdCategoria { get; set; }
}
