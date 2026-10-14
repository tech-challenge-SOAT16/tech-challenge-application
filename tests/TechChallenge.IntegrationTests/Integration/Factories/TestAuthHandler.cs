using System;
using System.Security.Claims;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace TechChallenge.IntegrationTests.Integration.Factories;

// Esquema de autenticação de teste: autentica todo request com a role do header "X-Test-Role"
// (default Administrador). Mantém os testes existentes verdes e permite testar RBAC por role.
public class TestAuthHandler : AuthenticationHandler<AuthenticationSchemeOptions>
{
    public const string SchemeName = "Test";
    public const string RoleHeader = "X-Test-Role";

    public TestAuthHandler(
        IOptionsMonitor<AuthenticationSchemeOptions> options,
        ILoggerFactory logger,
        UrlEncoder encoder)
        : base(options, logger, encoder)
    {
    }

    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        var role = Request.Headers[RoleHeader].FirstOrDefault() ?? "Administrador";

        var claims = new[]
        {
            new Claim("sub", Guid.NewGuid().ToString()),
            new Claim("role", role),
            new Claim("funcionarioId", Guid.NewGuid().ToString())
        };

        var identity = new ClaimsIdentity(claims, SchemeName, nameType: "sub", roleType: "role");
        var principal = new ClaimsPrincipal(identity);
        var ticket = new AuthenticationTicket(principal, SchemeName);

        return Task.FromResult(AuthenticateResult.Success(ticket));
    }
}
