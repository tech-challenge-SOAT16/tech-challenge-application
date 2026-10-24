using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using TechChallenge.Application.Abstractions.Auth;
using TechChallenge.Application.Abstractions.Notifications;
using TechChallenge.Application.Abstractions.Repositories;
using TechChallenge.Application.Features.Auth.Login;
using TechChallenge.Application.Features.Auth.Logout;
using TechChallenge.Application.Features.Auth.Refresh;
using TechChallenge.Application.Features.Auth.RefreshTokens;
using TechChallenge.Application.Features.Auth.TrocarSenha;
using TechChallenge.Application.Features.Usuarios.AlterarStatus;
using TechChallenge.Application.Features.Usuarios.AlterarTipo;
using TechChallenge.Application.Features.Usuarios.CriarUsuario;
using TechChallenge.Application.Features.Usuarios.DesvincularFuncionario;
using TechChallenge.Application.Features.Usuarios.ListarUsuarios;
using TechChallenge.Application.Features.Usuarios.ObterUsuario;
using TechChallenge.Application.Features.Usuarios.ResetarSenha;
using TechChallenge.Application.Features.Usuarios.VincularFuncionario;
using TechChallenge.Infrastructure.Database.Repositories;

namespace TechChallenge.Infrastructure.Auth;

public static class ConfigureServices
{
    public static IServiceCollection AddAuth(
        this IServiceCollection services,
        IConfiguration configuration,
        IHostEnvironment environment)
    {
        // Funcionários continuam autenticando pela própria API (emissão local
        // HS256 com chave do Secret, ou um emissor externo RS256). Clientes usam
        // o JWT da Function Serverless, no esquema "Customer".

        services.AddOptions<JwtOptions>()
            .Bind(configuration.GetSection(JwtOptions.SectionName))
            .Validate(options => !string.IsNullOrWhiteSpace(options.Issuer),
                "Jwt:Issuer é obrigatório.")
            .Validate(options => !string.IsNullOrWhiteSpace(options.Audience),
                "Jwt:Audience é obrigatório.")
            .Validate(options =>
                    options.EnableLocalIssuance ||
                    IsValidMetadataSource(options.Authority, options.MetadataAddress, options.RequireHttpsMetadata),
                "Configure Jwt:Authority ou Jwt:MetadataAddress com uma URL OIDC válida.")
            .Validate(options =>
                    !options.EnableLocalIssuance || options.LocalSigningKey.Length >= 32,
                "Jwt:LocalSigningKey deve ter no mínimo 32 caracteres quando a emissão local estiver habilitada.")
            .ValidateOnStart();

        services.AddOptions<CustomerAuthOptions>()
            .Bind(configuration.GetSection(CustomerAuthOptions.SectionName))
            .Validate(options => !options.Enabled ||
                    (!string.IsNullOrWhiteSpace(options.Issuer) && !string.IsNullOrWhiteSpace(options.Audience)),
                "CustomerAuth:Issuer e CustomerAuth:Audience são obrigatórios quando CustomerAuth:Enabled=true.")
            .Validate(options => !options.Enabled ||
                    IsValidMetadataSource(options.Authority, options.MetadataAddress, options.RequireHttpsMetadata),
                "Configure CustomerAuth:Authority ou CustomerAuth:MetadataAddress com uma URL OIDC válida.")
            .ValidateOnStart();

        services.AddOptions<ApprovalLinkTokenOptions>()
            .Bind(configuration.GetSection(ApprovalLinkTokenOptions.SectionName))
            .Validate(options => options.SigningKey.Length >= 32,
                "ApprovalLinks:SigningKey ausente ou com menos de 32 caracteres.")
            .ValidateOnStart();

        services.AddHttpContextAccessor();

        services.AddSingleton<IAuthSettings, AuthSettings>();
        services.AddSingleton<TimeProvider>(TimeProvider.System);
        services.AddSingleton<IDecisaoOrcamentoTokenService, DecisaoOrcamentoTokenService>();
        services.AddScoped<IPasswordHasher, Pbkdf2PasswordHasher>();
        services.AddScoped<ICurrentUser, CurrentUser>();

        services.AddScoped<IUsuarioRepository, UsuarioRepository>();
        services.AddScoped<IRefreshTokenRepository, RefreshTokenRepository>();

        // Estes serviços permanecem registrados para manter a composição estável
        // entre WebApplicationFactory, desenvolvimento e produção. As rotas locais
        // só são mapeadas com EnableLocalIssuance=true, e o TokenService também
        // bloqueia emissão quando a flag está desabilitada.
        services.AddSingleton<ITokenService, TokenService>();
        services.AddScoped<LoginService>();
        services.AddScoped<RefreshService>();
        services.AddScoped<TrocarSenhaService>();
        services.AddScoped<LogoutService>();
        services.AddScoped<ListarRefreshTokensService>();
        services.AddScoped<RevogarRefreshTokenService>();

        services.AddScoped<CriarUsuarioService>();
        services.AddScoped<ListarUsuariosService>();
        services.AddScoped<ObterUsuarioService>();
        services.AddScoped<AlterarTipoService>();
        services.AddScoped<VincularFuncionarioService>();
        services.AddScoped<DesvincularFuncionarioService>();
        services.AddScoped<AlterarStatusUsuarioService>();
        services.AddScoped<ResetarSenhaService>();

        // Os dois esquemas leem as opções já resolvidas (inclui overrides de ambiente
        // e de testes). Sem CustomerAuth:Enabled, o esquema "Customer" não tem chaves
        // e as rotas da área do cliente não são mapeadas.
        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer()
            .AddJwtBearer(CustomerAuthOptions.SchemeName, _ => { });

        services.AddOptions<JwtBearerOptions>(JwtBearerDefaults.AuthenticationScheme)
            .Configure<IOptions<JwtOptions>>((options, jwt) => ConfigureJwtBearer(options, jwt.Value));

        services.AddOptions<JwtBearerOptions>(CustomerAuthOptions.SchemeName)
            .Configure<IOptions<CustomerAuthOptions>>((options, customer) =>
            {
                if (customer.Value.Enabled)
                    ConfigureCustomerJwtBearer(options, customer.Value);
            });

        services.AddAuthorizationBuilder()
            // Fallback defensivo: endpoints sem política própria exigem um funcionário
            // autenticado com perfil; anônimos usam .AllowAnonymous(). O token de cliente
            // não passa aqui porque é validado só pelo esquema "Customer".
            .SetFallbackPolicy(new AuthorizationPolicyBuilder()
                .RequireAuthenticatedUser()
                .RequireRole("Administrador", "Vendedor", "Mecanico")
                .Build())
            .AddPolicy(CustomerAuthOptions.PolicyName, policy => policy
                .AddAuthenticationSchemes(CustomerAuthOptions.SchemeName)
                .RequireAuthenticatedUser()
                .RequireClaim(CustomerAuthOptions.ScopeClaim, CustomerAuthOptions.ScopeValue))
            .AddPolicy("AdminOnly", policy => policy.RequireRole("Administrador"))
            .AddPolicy("AdminOuVendedor", policy => policy.RequireRole("Administrador", "Vendedor"))
            .AddPolicy("MecanicoOuVendedor", policy => policy.RequireRole("Administrador", "Mecanico", "Vendedor"))
            .AddPolicy("Mecanico", policy => policy.RequireRole("Mecanico"));

        return services;
    }

    private static void ConfigureJwtBearer(JwtBearerOptions options, JwtOptions jwt)
    {
        options.MapInboundClaims = false;
        options.RequireHttpsMetadata = jwt.RequireHttpsMetadata;
        options.RefreshOnIssuerKeyNotFound = true;

        var validationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ClockSkew = TimeSpan.FromSeconds(30),
            ValidIssuer = jwt.Issuer,
            ValidAudience = jwt.Audience,
            NameClaimType = TokenService.ClaimNome,
            RoleClaimType = TokenService.ClaimRole
        };

        if (jwt.EnableLocalIssuance)
        {
            validationParameters.IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(jwt.LocalSigningKey));
            validationParameters.ValidAlgorithms = [SecurityAlgorithms.HmacSha256];
        }
        else
        {
            if (!string.IsNullOrWhiteSpace(jwt.Authority))
                options.Authority = jwt.Authority.TrimEnd('/');

            if (!string.IsNullOrWhiteSpace(jwt.MetadataAddress))
                options.MetadataAddress = jwt.MetadataAddress;

            // Defesa contra algorithm confusion: a API aceita somente tokens
            // RS256 assinados por uma chave pública anunciada no JWKS.
            validationParameters.ValidAlgorithms = [SecurityAlgorithms.RsaSha256];
        }

        options.TokenValidationParameters = validationParameters;
    }

    private static void ConfigureCustomerJwtBearer(JwtBearerOptions options, CustomerAuthOptions customer)
    {
        options.MapInboundClaims = false;
        options.RequireHttpsMetadata = customer.RequireHttpsMetadata;
        options.RefreshOnIssuerKeyNotFound = true;

        if (!string.IsNullOrWhiteSpace(customer.Authority))
            options.Authority = customer.Authority.TrimEnd('/');

        if (!string.IsNullOrWhiteSpace(customer.MetadataAddress))
            options.MetadataAddress = customer.MetadataAddress;

        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ClockSkew = TimeSpan.FromSeconds(30),
            ValidIssuer = customer.Issuer,
            ValidAudience = customer.Audience,
            // Somente RS256 com chave publicada no JWKS da Function.
            ValidAlgorithms = [SecurityAlgorithms.RsaSha256]
        };
    }

    private static bool IsValidMetadataSource(
        string authority,
        string metadataAddress,
        bool requireHttps)
    {
        var value = !string.IsNullOrWhiteSpace(metadataAddress) ? metadataAddress : authority;
        if (!Uri.TryCreate(value, UriKind.Absolute, out var uri))
            return false;

        return !requireHttps || uri.Scheme == Uri.UriSchemeHttps;
    }
}
