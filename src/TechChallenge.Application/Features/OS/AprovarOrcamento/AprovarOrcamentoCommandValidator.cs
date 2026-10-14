using FluentValidation;

namespace TechChallenge.Application.Features.OS.AprovarOrcamento;

public class AprovarOrcamentoCommandValidator : AbstractValidator<AprovarOrcamentoCommand>
{
    public AprovarOrcamentoCommandValidator()
    {
        RuleFor(command => command.OrdemServicoId)
            .NotEmpty();
    }
}
