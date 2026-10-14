using FluentValidation;

namespace TechChallenge.Application.Features.CategoriaProdutos.AtualizarCategoriaProduto;

public class AtualizarCategoriaProdutoCommandValidator : AbstractValidator<AtualizarCategoriaProdutoCommand>
{
    public AtualizarCategoriaProdutoCommandValidator()
    {
        RuleFor(command => command.Id)
            .NotEmpty();

        RuleFor(command => command.Descricao)
            .NotEmpty()
            .MaximumLength(100);
    }
}