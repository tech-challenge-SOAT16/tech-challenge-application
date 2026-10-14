namespace TechChallenge.Application.Abstractions.Auth
{
    public interface IAuthSettings
    {
        int RefreshTokenDays { get; }
    }
}
