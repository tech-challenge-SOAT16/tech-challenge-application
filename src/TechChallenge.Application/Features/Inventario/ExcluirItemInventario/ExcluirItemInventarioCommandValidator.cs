using FluentValidation;

namespace TechChallenge.Application.Features.Inventario.ExcluirItemInventario;

public class ExcluirItemInventarioCommandValidator : AbstractValidator<ExcluirItemInventarioCommand>
{
    public ExcluirItemInventarioCommandValidator()
    {
        RuleFor(command => command.Id)
            .NotEmpty();
    }
}
