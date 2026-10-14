namespace TechChallenge.Application.Features.Auth.TrocarSenha;

public class TrocarSenhaCommand
{
    public string SenhaAtual { get; set; } = string.Empty;
    public string NovaSenha { get; set; } = string.Empty;
}
