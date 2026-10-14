using FluentValidation;

namespace TechChallenge.Application.Features.CategoriaProdutos.CriarCategoriaProduto;

public class CriarCategoriaProdutoCommandValidator : AbstractValidator<CriarCategoriaProdutoCommand>
{
    public CriarCategoriaProdutoCommandValidator()
    {
        RuleFor(command => command.Descricao)
            .NotEmpty()
            .MaximumLength(100);
    }
}