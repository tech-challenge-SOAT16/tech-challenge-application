using FluentValidation;

namespace TechChallenge.Application.Features.CategoriaVeiculos.CriarCategoriaVeiculo;

public class CriarCategoriaVeiculoCommandValidator : AbstractValidator<CriarCategoriaVeiculoCommand>
{
    public CriarCategoriaVeiculoCommandValidator()
    {
        RuleFor(command => command.Descricao)
            .NotEmpty()
            .MaximumLength(100);
    }
}