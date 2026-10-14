using FluentValidation;

namespace TechChallenge.Application.Features.Estoque.BaixarEstoque;

public class BaixarEstoqueCommandValidator : AbstractValidator<BaixarEstoqueCommand>
{
    public BaixarEstoqueCommandValidator()
    {
        RuleFor(command => command.ProdutoId)
            .NotEmpty();

        RuleFor(command => command.Quantidade)
            .GreaterThan(0);
    }
}