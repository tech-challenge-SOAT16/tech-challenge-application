using System;
using TechChallenge.Domain.Enums;

namespace TechChallenge.Application.Features.Veiculos.CriarVeiculo;

public class CriarVeiculoCommand
{
    public string Placa { get; set; } = string.Empty;
    public string Modelo { get; set; } = string.Empty;
    public string Marca { get; set; } = string.Empty;
    public string Cor { get; set; } = string.Empty;
    public int Ano { get; set; }
    public decimal Quilometragem { get; set; }
    public decimal Valor { get; set; }
    public Guid ClienteId { get; set; }
    public Guid CategoriaId { get; set; }
}
