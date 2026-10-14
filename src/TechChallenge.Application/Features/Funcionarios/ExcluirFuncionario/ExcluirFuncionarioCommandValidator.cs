using FluentValidation;

namespace TechChallenge.Application.Features.Funcionarios.ExcluirFuncionario;

public class ExcluirFuncionarioCommandValidator : AbstractValidator<ExcluirFuncionarioCommand>
{
    public ExcluirFuncionarioCommandValidator()
    {
        RuleFor(command => command.Id)
            .NotEmpty();
    }
}
