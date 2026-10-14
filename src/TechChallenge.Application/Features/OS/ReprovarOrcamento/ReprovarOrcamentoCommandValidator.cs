using FluentValidation;

namespace TechChallenge.Application.Features.OS.ReprovarOrcamento;

public class ReprovarOrcamentoCommandValidator : AbstractValidator<ReprovarOrcamentoCommand>
{
    public ReprovarOrcamentoCommandValidator()
    {
        RuleFor(command => command.OrdemServicoId)
            .NotEmpty();
    }
}
