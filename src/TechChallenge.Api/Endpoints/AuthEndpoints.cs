using Microsoft.Extensions.Options;
using TechChallenge.Api.Models.Request;
using TechChallenge.Api.Models.Response;
using TechChallenge.Application.Abstractions.Auth;
using TechChallenge.Application.Features.Auth.Login;
using TechChallenge.Application.Features.Auth.Logout;
using TechChallenge.Application.Features.Auth.Refresh;
using TechChallenge.Application.Features.Auth.RefreshTokens;
using TechChallenge.Application.Features.Auth.TrocarSenha;
using TechChallenge.Application.Features.Usuarios.ObterUsuario;
using TechChallenge.Infrastructure.Auth;

namespace TechChallenge.Api.Endpoints;

public static class AuthEndpoints
{
    public static IEndpointRouteBuilder MapAuthEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/v1/auth").WithTags("Auth");
        var localIssuanceEnabled = app.ServiceProvider
            .GetRequiredService<IOptions<JwtOptions>>()
            .Value.EnableLocalIssuance;

        if (localIssuanceEnabled)
        {
            MapLocalAuthenticationEndpoints(group);
        }

        // Permanece disponível no modo OIDC/JWKS para retornar o usuário
        // identificado pelas claims emitidas pela Function Serverless.
        group.MapGet("/me", ObterUsuarioLogado)
            .RequireAuthorization()
            .WithName("ObterUsuarioLogado")
            .WithSummary("Retorna os dados do usuário autenticado")
            .Produces<UsuarioLogadoResponse>()
            .Produces(StatusCodes.Status401Unauthorized);

        return app;
    }

    private static void MapLocalAuthenticationEndpoints(RouteGroupBuilder group)
    {
        // Compatibilidade temporária restrita a desenvolvimento/testes.
        // Em Production, login, refresh e gestão de credenciais pertencem à
        // Function Serverless e estas rotas não são registradas.
        group.MapPost("/login", LoginAsync)
            .AllowAnonymous()
            .WithName("Login")
            .WithSummary("Autentica um usuário localmente (somente desenvolvimento/testes)")
            .Produces<LoginResponse>()
            .Produces(StatusCodes.Status401Unauthorized)
            .ProducesValidationProblem();

        group.MapPost("/refresh", RefreshAsync)
            .AllowAnonymous()
            .WithName("RefreshToken")
            .WithSummary("Renova um token local (somente desenvolvimento/testes)")
            .Produces<LoginResponse>()
            .Produces(StatusCodes.Status401Unauthorized);

        group.MapPost("/logout", LogoutAsync)
            .RequireAuthorization()
            .WithName("Logout")
            .WithSummary("Revoga os refresh tokens locais do usuário autenticado")
            .Produces(StatusCodes.Status204NoContent);

        group.MapPost("/logout-all", LogoutTodasAsync)
            .RequireAuthorization()
            .WithName("LogoutTodas")
            .WithSummary("Revoga todos os refresh tokens locais do usuário autenticado")
            .Produces(StatusCodes.Status204NoContent);

        group.MapPatch("/senha", TrocarSenhaAsync)
            .RequireAuthorization()
            .WithName("TrocarSenha")
            .WithSummary("Troca a senha local do usuário autenticado")
            .Produces(StatusCodes.Status204NoContent)
            .ProducesValidationProblem();

        group.MapGet("/refresh-tokens", ListarRefreshTokens)
            .RequireAuthorization()
            .WithName("ListarRefreshTokens")
            .WithSummary("Lista os refresh tokens locais ativos do usuário autenticado")
            .Produces<List<RefreshTokenResponse>>();

        group.MapDelete("/refresh-tokens/{refreshTokenId}", RevogarRefreshToken)
            .RequireAuthorization()
            .WithName("RevogarRefreshToken")
            .WithSummary("Revoga um refresh token local específico")
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status404NotFound);
    }

    private static async Task<IResult> LoginAsync(LoginRequest request, LoginService service)
    {
        var command = new LoginCommand
        {
            Login = request.Login,
            Senha = request.Senha
        };

        var resultado = await service.Login(command);
        return Results.Ok(MapToResponse(resultado));
    }

    private static async Task<IResult> RefreshAsync(RefreshRequest request, RefreshService service)
    {
        var resultado = await service.Refresh(new RefreshCommand { RefreshToken = request.RefreshToken });
        return Results.Ok(MapToResponse(resultado));
    }

    private static async Task<IResult> ObterUsuarioLogado(ICurrentUser currentUser, ObterUsuarioService service)
    {
        if (currentUser.UsuarioId is not { } usuarioId)
            return Results.Unauthorized();

        var usuario = await service.ObterUsuario(new ObterUsuarioQuery { Id = usuarioId });
        return Results.Ok(new UsuarioLogadoResponse(
            usuario.Id,
            usuario.Login,
            usuario.TipoUsuario.ToString(),
            usuario.FuncionarioId));
    }

    private static async Task<IResult> LogoutAsync(LogoutService service)
    {
        await service.Logout();
        return Results.NoContent();
    }

    private static async Task<IResult> LogoutTodasAsync(LogoutService service)
    {
        await service.LogoutTodas();
        return Results.NoContent();
    }

    private static async Task<IResult> TrocarSenhaAsync(TrocarSenhaRequest request, TrocarSenhaService service)
    {
        await service.TrocarSenha(new TrocarSenhaCommand
        {
            SenhaAtual = request.SenhaAtual,
            NovaSenha = request.NovaSenha
        });
        return Results.NoContent();
    }

    private static async Task<IResult> ListarRefreshTokens(ListarRefreshTokensService service)
    {
        var tokens = (await service.ListarRefreshTokens())
            .Select(s => new RefreshTokenResponse(s.Id, s.CriadoEm, s.ExpiraEm))
            .ToList();
        return Results.Ok(tokens);
    }

    private static async Task<IResult> RevogarRefreshToken(Guid refreshTokenId, RevogarRefreshTokenService service)
    {
        await service.RevogarRefreshToken(refreshTokenId);
        return Results.NoContent();
    }

    private static LoginResponse MapToResponse(Application.Features.Auth.AuthResult resultado) =>
        new(resultado.AccessToken, resultado.ExpiraEm, resultado.RefreshToken, "Bearer");
}
