using FluentValidation;

namespace TechChallenge.Application.Features.CategoriaServicos.AtualizarCategoriaServico;

public class AtualizarCategoriaServicoCommandValidator : AbstractValidator<AtualizarCategoriaServicoCommand>
{
    public AtualizarCategoriaServicoCommandValidator()
    {
        RuleFor(command => command.Id)
            .NotEmpty();

        RuleFor(command => command.Descricao)
            .NotEmpty()
            .MaximumLength(100);
    }
}