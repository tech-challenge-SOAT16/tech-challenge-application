using System.Diagnostics;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace TechChallenge.Infrastructure.Observability;

/// <summary>
/// Propaga o header x-correlation-id (mesmo contrato da Lambda de autenticação):
/// reaproveita o valor recebido quando válido, gera um novo caso contrário,
/// devolve na resposta e o inclui no escopo de log e no span atual.
/// </summary>
public sealed partial class CorrelationIdMiddleware
{
    public const string HeaderName = "x-correlation-id";
    public const string ItemKey = "CorrelationId";

    private readonly RequestDelegate _next;
    private readonly ILogger<CorrelationIdMiddleware> _logger;

    public CorrelationIdMiddleware(RequestDelegate next, ILogger<CorrelationIdMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var received = context.Request.Headers[HeaderName].ToString();
        var correlationId = ValidCorrelationId().IsMatch(received)
            ? received
            : Guid.NewGuid().ToString();

        context.Items[ItemKey] = correlationId;
        context.Response.OnStarting(() =>
        {
            context.Response.Headers[HeaderName] = correlationId;
            return Task.CompletedTask;
        });

        Activity.Current?.SetTag("correlation.id", correlationId);

        using (_logger.BeginScope("CorrelationId:{CorrelationId}", correlationId))
        {
            await _next(context);
        }
    }

    [GeneratedRegex("^[A-Za-z0-9-]{1,64}$")]
    private static partial Regex ValidCorrelationId();
}
