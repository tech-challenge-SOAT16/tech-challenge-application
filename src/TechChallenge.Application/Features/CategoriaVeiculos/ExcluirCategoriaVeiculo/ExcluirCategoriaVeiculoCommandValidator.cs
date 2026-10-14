using FluentValidation;

namespace TechChallenge.Application.Features.CategoriaVeiculos.ExcluirCategoriaVeiculo;

public class ExcluirCategoriaVeiculoCommandValidator : AbstractValidator<ExcluirCategoriaVeiculoCommand>
{
    public ExcluirCategoriaVeiculoCommandValidator()
    {
        RuleFor(command => command.Id)
            .NotEmpty();
    }
}