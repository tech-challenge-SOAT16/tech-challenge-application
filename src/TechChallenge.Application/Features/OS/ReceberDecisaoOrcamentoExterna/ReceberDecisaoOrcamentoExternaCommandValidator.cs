using FluentValidation;
using TechChallenge.Domain.Enums;

namespace TechChallenge.Application.Features.OS.ReceberDecisaoOrcamentoExterna;

public class ReceberDecisaoOrcamentoExternaCommandValidator : AbstractValidator<ReceberDecisaoOrcamentoExternaCommand>
{
    public ReceberDecisaoOrcamentoExternaCommandValidator()
    {
        RuleFor(command => command.EventoId)
            .NotEmpty()
            .MaximumLength(100);

        RuleFor(command => command.OrdemServicoId)
            .NotEmpty();

        RuleFor(command => command.Decisao)
            .IsInEnum();

        RuleFor(command => command.Motivo)
            .NotEmpty()
            .When(command => command.Decisao == DecisaoOrcamento.Recusado)
            .WithMessage("O motivo é obrigatório quando o orçamento é recusado.");

        RuleFor(command => command.Motivo)
            .MaximumLength(500);

        RuleFor(command => command.OcorridoEm)
            .NotEmpty()
            .Must(data => data <= DateTimeOffset.UtcNow.AddMinutes(5))
            .WithMessage("A data do evento não pode estar no futuro.");
    }
}
