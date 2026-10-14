using FluentValidation;

namespace TechChallenge.Application.Features.Inventario.AtualizarItemInventario;

public class AtualizarItemInventarioCommandValidator : AbstractValidator<AtualizarItemInventarioCommand>
{
    public AtualizarItemInventarioCommandValidator()
    {
        RuleFor(command => command.Id)
            .NotEmpty();

        RuleFor(command => command.Descricao)
            .NotEmpty()
            .MaximumLength(100);

        RuleFor(command => command.Valor)
            .GreaterThanOrEqualTo(0);

        RuleFor(command => command.Quantidade)
            .GreaterThanOrEqualTo(0);
    }
}
