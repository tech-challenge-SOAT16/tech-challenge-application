using System;
namespace TechChallenge.Application.Features.OS.RegistrarDiagnostico;

public class RegistrarDiagnosticoCommand
{
    public Guid OrdemServicoId { get; set; }
    public List<ItemDiagnosticoCommand> Servicos { get; set; } = [];
    public List<ItemDiagnosticoCommand> Produtos { get; set; } = [];
}

public class ItemDiagnosticoCommand
{
    public Guid Id { get; set; }
    public int Quantidade { get; set; } = 1;
}
