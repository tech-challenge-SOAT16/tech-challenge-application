using FluentValidation;

namespace TechChallenge.Application.Features.Auth.Login;

public class LoginCommandValidator : AbstractValidator<LoginCommand>
{
    public LoginCommandValidator()
    {
        RuleFor(command => command.Login).NotEmpty();
        RuleFor(command => command.Senha).NotEmpty();
    }
}
