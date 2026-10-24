namespace TechChallenge.Infrastructure.Auth;

/// <summary>
/// Autenticação de clientes pelo JWT RS256 emitido pela Function Serverless
/// (login por CPF). É um esquema separado do JWT de funcionários: o token de
/// cliente só é aceito nas rotas da área do cliente.
/// </summary>
public sealed class CustomerAuthOptions
{
    public const string SectionName = "CustomerAuth";
    public const string SchemeName = "Customer";
    public const string PolicyName = "Cliente";
    public const string ScopeClaim = "scope";
    public const string ScopeValue = "customer";

    public bool Enabled { get; set; }

    // URL do emissor (API Gateway da Lambda). O JwtBearer consulta
    // {Authority}/.well-known/openid-configuration e obtém o JWKS.
    public string Authority { get; set; } = string.Empty;
    public string MetadataAddress { get; set; } = string.Empty;
    public string Issuer { get; set; } = string.Empty;
    public string Audience { get; set; } = "tech-challenge-api";
    public bool RequireHttpsMetadata { get; set; } = true;
}
