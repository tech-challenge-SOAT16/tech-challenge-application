using FluentValidation;

namespace TechChallenge.Application.Features.OS.AtribuirOS;

public class AtribuirOSCommandValidator : AbstractValidator<AtribuirOSCommand>
{
    public AtribuirOSCommandValidator()
    {
        RuleFor(command => command.OrdemServicoId)
            .NotEmpty();

        RuleFor(command => command.MecanicoId)
            .NotEmpty();
    }
}
