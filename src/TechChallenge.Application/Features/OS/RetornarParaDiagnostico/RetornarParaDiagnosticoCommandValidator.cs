using FluentValidation;

namespace TechChallenge.Application.Features.OS.RetornarParaDiagnostico;

public class RetornarParaDiagnosticoCommandValidator : AbstractValidator<RetornarParaDiagnosticoCommand>
{
    public RetornarParaDiagnosticoCommandValidator()
    {
        RuleFor(command => command.OrdemServicoId)
            .NotEmpty();
    }
}
