using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Options;
using TechChallenge.Api.Configuration;

namespace TechChallenge.Api.Filters;

public class IntegrationApiKeyFilter : IEndpointFilter
{
    private readonly IntegracaoExternaOptions _options;

    public IntegrationApiKeyFilter(IOptions<IntegracaoExternaOptions> options)
    {
        _options = options.Value;
    }

    public async ValueTask<object?> InvokeAsync(
        EndpointFilterInvocationContext context,
        EndpointFilterDelegate next)
    {
        if (string.IsNullOrWhiteSpace(_options.ApiKey))
        {
            return Results.Problem(
                statusCode: StatusCodes.Status503ServiceUnavailable,
                title: "Integração externa indisponível.",
                detail: "A chave da integração externa não foi configurada.");
        }

        if (!context.HttpContext.Request.Headers.TryGetValue(IntegracaoExternaOptions.HeaderName, out var values) ||
            !ChavesIguais(values.ToString(), _options.ApiKey))
        {
            return Results.Problem(
                statusCode: StatusCodes.Status401Unauthorized,
                title: "Chave de integração inválida.");
        }

        return await next(context);
    }

    private static bool ChavesIguais(string fornecida, string esperada)
    {
        var bytesFornecidos = Encoding.UTF8.GetBytes(fornecida);
        var bytesEsperados = Encoding.UTF8.GetBytes(esperada);

        return bytesFornecidos.Length == bytesEsperados.Length &&
               CryptographicOperations.FixedTimeEquals(bytesFornecidos, bytesEsperados);
    }
}
