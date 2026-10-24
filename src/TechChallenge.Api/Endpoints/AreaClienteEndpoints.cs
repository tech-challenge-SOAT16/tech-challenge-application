using System.Security.Claims;
using Microsoft.Extensions.Options;
using TechChallenge.Api.Models.Response;
using TechChallenge.Application.Features.OS.AreaCliente;
using TechChallenge.Infrastructure.Auth;

namespace TechChallenge.Api.Endpoints;

/// <summary>
/// Rotas do cliente autenticado pelo JWT da Function Serverless (login por CPF).
/// O "sub" do token é o Id do cliente.
/// </summary>
public static class AreaClienteEndpoints
{
    public static IEndpointRouteBuilder MapAreaClienteEndpoints(this IEndpointRouteBuilder app)
    {
        var enabled = app.ServiceProvider
            .GetRequiredService<IOptions<CustomerAuthOptions>>()
            .Value.Enabled;

        if (!enabled)
            return app;

        var group = app.MapGroup("/api/v1/cliente/ordens-servico")
            .WithTags("Área do cliente")
            .RequireAuthorization(CustomerAuthOptions.PolicyName);

        group.MapGet("/", ListarAsync)
            .WithName("ListarOrdensServicoDoCliente")
            .WithSummary("Lista as ordens de serviço do cliente autenticado")
            .Produces<List<OrdemServicoResponse>>()
            .Produces(StatusCodes.Status401Unauthorized);

        group.MapGet("/{id:guid}", ObterAsync)
            .WithName("ObterOrdemServicoDoCliente")
            .WithSummary("Obtém uma ordem de serviço do cliente autenticado")
            .Produces<OrdemServicoResponse>()
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces(StatusCodes.Status404NotFound);

        return app;
    }

    private static async Task<IResult> ListarAsync(ClaimsPrincipal user, OrdensServicoClienteService service)
    {
        if (!TryGetClienteId(user, out var clienteId))
            return Results.Unauthorized();

        var ordens = await service.ListarAsync(clienteId);
        return Results.Ok(ordens.Select(OrdensServicoEndpoints.MapToResponse).ToList());
    }

    private static async Task<IResult> ObterAsync(Guid id, ClaimsPrincipal user, OrdensServicoClienteService service)
    {
        if (!TryGetClienteId(user, out var clienteId))
            return Results.Unauthorized();

        var ordem = await service.ObterAsync(clienteId, id);
        return Results.Ok(OrdensServicoEndpoints.MapToResponse(ordem));
    }

    private static bool TryGetClienteId(ClaimsPrincipal user, out Guid clienteId) =>
        Guid.TryParse(user.FindFirstValue("sub"), out clienteId);
}
