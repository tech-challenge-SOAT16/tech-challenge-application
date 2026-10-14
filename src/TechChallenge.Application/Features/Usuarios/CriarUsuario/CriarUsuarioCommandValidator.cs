using FluentValidation;
using TechChallenge.Application.Validation;

namespace TechChallenge.Application.Features.Usuarios.CriarUsuario;

public class CriarUsuarioCommandValidator : AbstractValidator<CriarUsuarioCommand>
{
    public CriarUsuarioCommandValidator()
    {
        RuleFor(command => command.Login)
            .NotEmpty()
            .MaximumLength(100);

        RuleFor(command => command.Senha).AplicarPoliticaSenha();

        RuleFor(command => command.TipoUsuario).IsInEnum();
    }
}
