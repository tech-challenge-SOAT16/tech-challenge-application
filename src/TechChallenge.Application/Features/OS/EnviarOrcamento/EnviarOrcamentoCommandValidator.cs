using FluentValidation;

namespace TechChallenge.Application.Features.OS.EnviarOrcamento;

public class EnviarOrcamentoCommandValidator : AbstractValidator<EnviarOrcamentoCommand>
{
    public EnviarOrcamentoCommandValidator()
    {
        RuleFor(command => command.OrdemServicoId)
            .NotEmpty();
    }
}
