using System;
using TechChallenge.Domain.Enums;

namespace TechChallenge.Api.Models.Response;

public class OrdemServicoResponse
{
    public Guid Id { get; set; }
    public string Descricao { get; set; } = string.Empty;
    public StatusOS Status { get; set; }
    public Guid ClienteResponsavelId { get; set; }
    public Guid FuncionarioResponsavelId { get; set; }
    public Guid VeiculoId { get; set; }
    public string CodigoAcompanhamento { get; set; } = string.Empty;
    public DateTime DataCriacao { get; set; }
    public DateTime? DataAtualizacao { get; set; }
    public DateTime? DataFinalizacao { get; set; }
    public double Valor { get; set; }
    public List<ServicoResponse> Servicos { get; set; } = [];
    public List<ProdutoResponse> Produtos { get; set; } = [];
}
