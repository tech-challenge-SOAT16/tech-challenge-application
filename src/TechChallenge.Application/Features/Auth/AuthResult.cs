using System;
namespace TechChallenge.Application.Features.Auth;

public record AuthResult(string AccessToken, DateTime ExpiraEm, string RefreshToken);
