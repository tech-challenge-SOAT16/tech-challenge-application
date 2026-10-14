namespace TechChallenge.Infrastructure.Auth;

public sealed class ApprovalLinkTokenOptions
{
    public const string SectionName = "ApprovalLinks";

    // Chave HMAC exclusiva dos links de aprovação/recusa. Não é material do
    // JWT bearer e deve ser rotacionada independentemente das chaves da Function.
    public string SigningKey { get; set; } = string.Empty;
}
