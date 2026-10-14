using System;
using TechChallenge.Domain.Enums;

namespace TechChallenge.Application.Features.Usuarios.CriarUsuario;

public class CriarUsuarioCommand
{
    public string Login { get; set; } = string.Empty;
    public string Senha { get; set; } = string.Empty;
    public TipoUsuario TipoUsuario { get; set; }
    public Guid? FuncionarioId { get; set; }
}
