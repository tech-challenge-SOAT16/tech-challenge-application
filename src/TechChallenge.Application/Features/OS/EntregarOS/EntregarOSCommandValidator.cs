using FluentValidation;

namespace TechChallenge.Application.Features.OS.EntregarOS;

public class EntregarOSCommandValidator : AbstractValidator<EntregarOSCommand>
{
    public EntregarOSCommandValidator()
    {
        RuleFor(command => command.OrdemServicoId)
            .NotEmpty();
    }
}
