using FluentValidation;

namespace TechChallenge.Application.Features.OS.FinalizarOS;

public class FinalizarOSCommandValidator : AbstractValidator<FinalizarOSCommand>
{
    public FinalizarOSCommandValidator()
    {
        RuleFor(command => command.OrdemServicoId)
            .NotEmpty();
    }
}
