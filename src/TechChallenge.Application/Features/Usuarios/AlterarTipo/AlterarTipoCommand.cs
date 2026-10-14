using System;
using TechChallenge.Domain.Enums;

namespace TechChallenge.Application.Features.Usuarios.AlterarTipo;

public class AlterarTipoCommand
{
    public Guid UsuarioId { get; set; }
    public TipoUsuario TipoUsuario { get; set; }
}
