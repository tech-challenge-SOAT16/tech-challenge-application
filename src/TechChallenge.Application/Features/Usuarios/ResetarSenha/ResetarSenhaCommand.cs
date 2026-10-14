using System;
namespace TechChallenge.Application.Features.Usuarios.ResetarSenha;

public class ResetarSenhaCommand
{
    public Guid UsuarioId { get; set; }
    public string NovaSenha { get; set; } = string.Empty;
}
