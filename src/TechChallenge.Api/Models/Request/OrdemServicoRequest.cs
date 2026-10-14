using System;
namespace TechChallenge.Api.Models.Request;

public class CriarOrdemServicoRequest
{
    public string Descricao { get; set; } = string.Empty;
    public Guid ClienteResponsavelId { get; set; }
    public Guid FuncionarioResponsavelId { get; set; }
    public Guid VeiculoId { get; set; }
    public List<ItemDiagnosticoRequest> Servicos { get; set; } = [];
    public List<ItemDiagnosticoRequest> Produtos { get; set; } = [];
}

public class AtribuirOrdemServicoRequest
{
    public Guid MecanicoId { get; set; }
}

public class RegistrarDiagnosticoRequest
{
    public List<ItemDiagnosticoRequest> Servicos { get; set; } = [];
    public List<ItemDiagnosticoRequest> Produtos { get; set; } = [];
}

public class ItemDiagnosticoRequest
{
    public Guid Id { get; set; }
    public int Quantidade { get; set; } = 1;
}
