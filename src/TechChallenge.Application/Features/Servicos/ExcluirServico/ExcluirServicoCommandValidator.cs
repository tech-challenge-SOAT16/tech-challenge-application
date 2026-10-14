using FluentValidation;

namespace TechChallenge.Application.Features.Servicos.ExcluirServico;

public class ExcluirServicoCommandValidator : AbstractValidator<ExcluirServicoCommand>
{
    public ExcluirServicoCommandValidator()
    {
        RuleFor(command => command.Id)
            .NotEmpty();
    }
}
