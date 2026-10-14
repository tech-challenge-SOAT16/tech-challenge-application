using System;
namespace TechChallenge.Application.Features.OS.AtribuirOS;

public class AtribuirOSCommand
{
    public Guid OrdemServicoId { get; set; }
    public Guid MecanicoId { get; set; }
}