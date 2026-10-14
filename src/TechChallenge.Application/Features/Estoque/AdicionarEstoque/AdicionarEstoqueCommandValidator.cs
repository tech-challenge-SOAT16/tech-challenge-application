using FluentValidation;

namespace TechChallenge.Application.Features.Estoque.AdicionarEstoque;

public class AdicionarEstoqueCommandValidator : AbstractValidator<AdicionarEstoqueCommand>
{
    public AdicionarEstoqueCommandValidator()
    {
        RuleFor(command => command.ProdutoId)
            .NotEmpty();

        RuleFor(command => command.Quantidade)
            .GreaterThan(0);
    }
}