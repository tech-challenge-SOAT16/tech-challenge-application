using FluentValidation;

namespace TechChallenge.Application.Features.OS.CancelarOS;

public class CancelarOSCommandValidator : AbstractValidator<CancelarOSCommand>
{
    public CancelarOSCommandValidator()
    {
        RuleFor(command => command.OrdemServicoId)
            .NotEmpty();
    }
}
