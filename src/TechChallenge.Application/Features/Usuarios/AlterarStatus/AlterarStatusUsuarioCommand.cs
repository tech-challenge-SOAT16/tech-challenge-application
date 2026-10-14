using System;
namespace TechChallenge.Application.Features.Usuarios.AlterarStatus;

public class AlterarStatusUsuarioCommand
{
    public Guid UsuarioId { get; set; }
    public bool Ativo { get; set; }
}
