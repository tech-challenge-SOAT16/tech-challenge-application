namespace TechChallenge.Infrastructure.Auth;

public sealed class JwtOptions
{
    public const string SectionName = "Jwt";

    public string Issuer { get; set; } = string.Empty;
    public string Audience { get; set; } = string.Empty;

    // Base URL do emissor OIDC. Por padrão, o JwtBearer consulta
    // {Authority}/.well-known/openid-configuration e obtém o jwks_uri.
    public string Authority { get; set; } = string.Empty;

    // Opcional: use somente quando o discovery não estiver no endereço padrão.
    public string MetadataAddress { get; set; } = string.Empty;
    public bool RequireHttpsMetadata { get; set; } = true;

    // Emissão local (HS256) dos tokens de funcionários, com chave vinda do
    // Secret. Clientes usam o JWT da Function Serverless (CustomerAuth).
    public bool EnableLocalIssuance { get; set; }
    public string LocalSigningKey { get; set; } = string.Empty;

    public int AccessTokenMinutes { get; set; } = 15;
    public int RefreshTokenDays { get; set; } = 7;
}
