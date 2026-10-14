using FluentValidation;

namespace TechChallenge.Application.Validation;

public static class SenhaValidator
{
    // Política de senha: mínimo 8 caracteres, ao menos 1 letra e 1 número.
    public static IRuleBuilderOptions<T, string> AplicarPoliticaSenha<T>(this IRuleBuilder<T, string> rule)
    {
        return rule
            .NotEmpty()
            .MinimumLength(8).WithMessage("A senha deve ter no mínimo 8 caracteres.")
            .Matches("[A-Za-z]").WithMessage("A senha deve conter ao menos uma letra.")
            .Matches("[0-9]").WithMessage("A senha deve conter ao menos um número.");
    }
}
