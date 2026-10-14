using System;
namespace TechChallenge.Application.Features.OS.CriarOS;

public class CriarOSCommand
{
    public string Descricao { get; set; } = string.Empty;
    public Guid ClienteResponsavelId { get; set; }
    public Guid FuncionarioResponsavelId { get; set; }
    public Guid VeiculoId { get; set; }
    public List<ItemOrdemServicoCommand> Servicos { get; set; } = [];
    public List<ItemOrdemServicoCommand> Produtos { get; set; } = [];
}

public class ItemOrdemServicoCommand
{
    public Guid Id { get; set; }
    public int Quantidade { get; set; } = 1;
}
