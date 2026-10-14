using System;
using TechChallenge.Domain.Entities;

namespace TechChallenge.Application.Abstractions.Auth
{
    public record AccessTokenResult(string Token, DateTime ExpiraEm);

    public interface ITokenService
    {
        AccessTokenResult GerarAccessToken(Usuario usuario);
        string GerarRefreshToken();                 // valor cru aleatório (256-bit)
        string HashRefreshToken(string tokenCru);   // SHA-256 -> armazenar/comparar
    }
}
