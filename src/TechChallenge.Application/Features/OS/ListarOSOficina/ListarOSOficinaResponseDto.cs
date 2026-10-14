using System;

namespace TechChallenge.Application.Features.OS.ListarOSOficina;

public class ListarOSOficinaResponseDto
{
    public Guid Id { get; set; }
    public string PlacaVeiculo { get; set; } = string.Empty;
    public string NomeMecanico { get; set; } = string.Empty;
    public string RelatoInicial { get; set; } = string.Empty;
}
