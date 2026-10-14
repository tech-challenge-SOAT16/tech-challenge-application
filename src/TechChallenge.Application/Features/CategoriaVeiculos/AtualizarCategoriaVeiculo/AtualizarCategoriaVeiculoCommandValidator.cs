using FluentValidation;

namespace TechChallenge.Application.Features.CategoriaVeiculos.AtualizarCategoriaVeiculo;

public class AtualizarCategoriaVeiculoCommandValidator : AbstractValidator<AtualizarCategoriaVeiculoCommand>
{
    public AtualizarCategoriaVeiculoCommandValidator()
    {
        RuleFor(command => command.Id)
            .NotEmpty();

        RuleFor(command => command.Descricao)
            .NotEmpty()
            .MaximumLength(100);
    }
}