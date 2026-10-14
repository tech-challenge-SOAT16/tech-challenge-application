using FluentValidation;
using TechChallenge.Application.Validation;

namespace TechChallenge.Application.Features.Usuarios.ResetarSenha;

public class ResetarSenhaCommandValidator : AbstractValidator<ResetarSenhaCommand>
{
    public ResetarSenhaCommandValidator()
    {
        RuleFor(command => command.UsuarioId).NotEmpty();
        RuleFor(command => command.NovaSenha).AplicarPoliticaSenha();
    }
}
