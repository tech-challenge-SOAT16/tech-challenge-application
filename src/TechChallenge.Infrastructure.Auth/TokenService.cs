using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using TechChallenge.Application.Abstractions.Auth;
using TechChallenge.Domain.Entities;

namespace TechChallenge.Infrastructure.Auth
{
    public class TokenService : ITokenService
    {
        public const string ClaimSub = "sub";
        public const string ClaimRole = "role";
        public const string ClaimNome = "name";
        public const string ClaimFuncionarioId = "funcionarioId";

        private readonly JwtOptions _options;

        public TokenService(IOptions<JwtOptions> options)
        {
            _options = options.Value;
        }

        public AccessTokenResult GerarAccessToken(Usuario usuario)
        {
            if (!_options.EnableLocalIssuance)
            {
                throw new InvalidOperationException(
                    "A emissão local de access tokens está desabilitada. Use a Function Serverless.");
            }

            var agora = DateTime.UtcNow;
            var expira = agora.AddMinutes(_options.AccessTokenMinutes);

            var claims = new List<Claim>
            {
                new(ClaimSub, usuario.Id.ToString()),
                new(ClaimNome, usuario.Login),
                new(ClaimRole, usuario.TipoUsuario.ToString()),
                new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };

            if (usuario.FuncionarioId is { } funcionarioId)
                claims.Add(new Claim(ClaimFuncionarioId, funcionarioId.ToString()));

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_options.LocalSigningKey));
            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: _options.Issuer,
                audience: _options.Audience,
                claims: claims,
                notBefore: agora,
                expires: expira,
                signingCredentials: credentials);

            var jwt = new JwtSecurityTokenHandler().WriteToken(token);
            return new AccessTokenResult(jwt, expira);
        }

        public string GerarRefreshToken()
        {
            var bytes = RandomNumberGenerator.GetBytes(32);
            return Base64UrlEncoder.Encode(bytes);
        }

        public string HashRefreshToken(string tokenCru)
        {
            var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(tokenCru));
            return Convert.ToBase64String(bytes);
        }
    }
}
