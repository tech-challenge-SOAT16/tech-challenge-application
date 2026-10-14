using FluentValidation;

namespace TechChallenge.Application.Features.CategoriaServicos.CriarCategoriaServico;

public class CriarCategoriaServicoCommandValidator : AbstractValidator<CriarCategoriaServicoCommand>
{
    public CriarCategoriaServicoCommandValidator()
    {
        RuleFor(command => command.Descricao)
            .NotEmpty()
            .MaximumLength(100);
    }
}