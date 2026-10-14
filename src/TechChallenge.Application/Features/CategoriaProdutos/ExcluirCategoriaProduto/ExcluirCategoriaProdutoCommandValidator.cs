using FluentValidation;

namespace TechChallenge.Application.Features.CategoriaProdutos.ExcluirCategoriaProduto;

public class ExcluirCategoriaProdutoCommandValidator : AbstractValidator<ExcluirCategoriaProdutoCommand>
{
    public ExcluirCategoriaProdutoCommandValidator()
    {
        RuleFor(command => command.Id)
            .NotEmpty();
    }
}