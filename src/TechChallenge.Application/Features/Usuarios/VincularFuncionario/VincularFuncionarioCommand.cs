using System;
namespace TechChallenge.Application.Features.Usuarios.VincularFuncionario;

public class VincularFuncionarioCommand
{
    public Guid UsuarioId { get; set; }
    public Guid FuncionarioId { get; set; }
}
