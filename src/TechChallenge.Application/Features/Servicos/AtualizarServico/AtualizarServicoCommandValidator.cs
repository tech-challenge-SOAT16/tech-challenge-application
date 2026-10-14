using FluentValidation;

namespace TechChallenge.Application.Features.Servicos.AtualizarServico;

public class AtualizarServicoCommandValidator : AbstractValidator<AtualizarServicoCommand>
{
    public AtualizarServicoCommandValidator()
    {
        RuleFor(command => command.Id)
            .NotEmpty();

        RuleFor(command => command.Descricao)
            .NotEmpty()
            .MaximumLength(100);

        RuleFor(command => command.Valor)
            .GreaterThanOrEqualTo(0);
    }
}
