using System;
namespace TechChallenge.Api.Models.Request;

public class CriarUsuarioRequest
{
    public string Login { get; set; } = string.Empty;
    public string Senha { get; set; } = string.Empty;
    public string TipoUsuario { get; set; } = string.Empty;
    public Guid? FuncionarioId { get; set; }
}

public record AlterarTipoRequest(string TipoUsuario);

public record VincularFuncionarioRequest(Guid FuncionarioId);

public record ResetarSenhaRequest(string NovaSenha);
