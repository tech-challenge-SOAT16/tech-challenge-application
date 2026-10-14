using FluentValidation;
using TechChallenge.Application.Validation;

namespace TechChallenge.Application.Features.Auth.TrocarSenha;

public class TrocarSenhaCommandValidator : AbstractValidator<TrocarSenhaCommand>
{
    public TrocarSenhaCommandValidator()
    {
        RuleFor(command => command.SenhaAtual).NotEmpty();
        RuleFor(command => command.NovaSenha).AplicarPoliticaSenha();
        RuleFor(command => command.NovaSenha)
            .NotEqual(command => command.SenhaAtual)
            .WithMessage("A nova senha deve ser diferente da atual.");
    }
}
