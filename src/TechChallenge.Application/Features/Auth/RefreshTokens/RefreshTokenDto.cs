using System;
namespace TechChallenge.Application.Features.Auth.RefreshTokens;

public record RefreshTokenDto(
    Guid Id,
    DateTime CriadoEm,
    DateTime ExpiraEm);
