using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Options;
using TechChallenge.Application.Abstractions.Notifications;

namespace TechChallenge.Infrastructure.Auth;

public sealed class DecisaoOrcamentoTokenService : IDecisaoOrcamentoTokenService
{
    private const string Purpose = "decisao-orcamento-v1";

    private readonly byte[] _signingKey;
    private readonly TimeProvider _timeProvider;

    public DecisaoOrcamentoTokenService(
        IOptions<ApprovalLinkTokenOptions> options,
        TimeProvider timeProvider)
    {
        _signingKey = Encoding.UTF8.GetBytes(options.Value.SigningKey);
        _timeProvider = timeProvider;
    }

    public string Gerar(
        Guid eventoId,
        Guid ordemServicoId,
        DateTimeOffset emitidoEm,
        TimeSpan validade)
    {
        if (eventoId == Guid.Empty || ordemServicoId == Guid.Empty)
            throw new ArgumentException("Evento e ordem de serviço são obrigatórios para gerar o token.");

        if (validade <= TimeSpan.Zero)
            throw new ArgumentOutOfRangeException(nameof(validade), "A validade do token deve ser positiva.");

        var emissaoUtc = emitidoEm.ToUniversalTime();
        var expiracaoUtc = emissaoUtc.Add(validade);
        var payload = string.Join(
            '|',
            Purpose,
            eventoId.ToString("N"),
            ordemServicoId.ToString("N"),
            emissaoUtc.ToUnixTimeSeconds().ToString(CultureInfo.InvariantCulture),
            expiracaoUtc.ToUnixTimeSeconds().ToString(CultureInfo.InvariantCulture));

        var payloadBytes = Encoding.UTF8.GetBytes(payload);
        var assinatura = HMACSHA256.HashData(_signingKey, payloadBytes);

        return $"{Base64UrlEncode(payloadBytes)}.{Base64UrlEncode(assinatura)}";
    }

    public DecisaoOrcamentoToken? Validar(string token)
    {
        if (string.IsNullOrWhiteSpace(token))
            return null;

        var partes = token.Split('.');
        if (partes.Length != 2)
            return null;

        try
        {
            var payloadBytes = Base64UrlDecode(partes[0]);
            var assinaturaRecebida = Base64UrlDecode(partes[1]);

            if (Base64UrlEncode(payloadBytes) != partes[0] ||
                Base64UrlEncode(assinaturaRecebida) != partes[1])
            {
                return null;
            }

            var assinaturaEsperada = HMACSHA256.HashData(_signingKey, payloadBytes);

            if (!CryptographicOperations.FixedTimeEquals(assinaturaRecebida, assinaturaEsperada))
                return null;

            var campos = Encoding.UTF8.GetString(payloadBytes).Split('|');
            if (campos.Length != 5 || campos[0] != Purpose ||
                !Guid.TryParseExact(campos[1], "N", out var eventoId) ||
                !Guid.TryParseExact(campos[2], "N", out var ordemServicoId) ||
                !long.TryParse(campos[3], CultureInfo.InvariantCulture, out var emissaoUnix) ||
                !long.TryParse(campos[4], CultureInfo.InvariantCulture, out var expiracaoUnix))
            {
                return null;
            }

            var emitidoEm = DateTimeOffset.FromUnixTimeSeconds(emissaoUnix);
            var expiraEm = DateTimeOffset.FromUnixTimeSeconds(expiracaoUnix);
            var agora = _timeProvider.GetUtcNow();

            if (expiraEm <= agora || expiraEm <= emitidoEm || emitidoEm > agora.AddMinutes(5))
                return null;

            return new DecisaoOrcamentoToken(
                eventoId,
                ordemServicoId,
                emitidoEm,
                expiraEm);
        }
        catch (FormatException)
        {
            return null;
        }
        catch (ArgumentOutOfRangeException)
        {
            return null;
        }
    }

    private static string Base64UrlEncode(byte[] bytes) =>
        Convert.ToBase64String(bytes)
            .TrimEnd('=')
            .Replace('+', '-')
            .Replace('/', '_');

    private static byte[] Base64UrlDecode(string value)
    {
        var base64 = value.Replace('-', '+').Replace('_', '/');
        base64 = (base64.Length % 4) switch
        {
            2 => base64 + "==",
            3 => base64 + "=",
            _ => base64
        };

        return Convert.FromBase64String(base64);
    }
}
