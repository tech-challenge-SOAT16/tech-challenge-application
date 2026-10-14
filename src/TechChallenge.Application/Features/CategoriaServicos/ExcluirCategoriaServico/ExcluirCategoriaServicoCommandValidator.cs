using FluentValidation;

namespace TechChallenge.Application.Features.CategoriaServicos.ExcluirCategoriaServico;

public class ExcluirCategoriaServicoCommandValidator : AbstractValidator<ExcluirCategoriaServicoCommand>
{
    public ExcluirCategoriaServicoCommandValidator()
    {
        RuleFor(command => command.Id)
            .NotEmpty();
    }
}