namespace TechChallenge.Api.Models.Request;

public class LoginRequest
{
    public string Login { get; set; } = string.Empty;
    public string Senha { get; set; } = string.Empty;
}
