using FluentValidation;

namespace TechChallenge.Application.Features.OS.ExcluirOS;

public class ExcluirOSCommandValidator : AbstractValidator<ExcluirOSCommand>
{
    public ExcluirOSCommandValidator()
    {
        RuleFor(command => command.Id)
            .NotEmpty();
    }
}
