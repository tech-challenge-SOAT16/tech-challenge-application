using Microsoft.Extensions.Options;
using TechChallenge.Application.Abstractions.Auth;

namespace TechChallenge.Infrastructure.Auth
{
    public class AuthSettings : IAuthSettings
    {
        private readonly JwtOptions _options;

        public AuthSettings(IOptions<JwtOptions> options)
        {
            _options = options.Value;
        }

        public int RefreshTokenDays => _options.RefreshTokenDays;
    }
}
