using FluentValidation;

namespace TechChallenge.Application.Features.Veiculos.ExcluirVeiculo;

public class ExcluirVeiculoCommandValidator : AbstractValidator<ExcluirVeiculoCommand>
{
    public ExcluirVeiculoCommandValidator()
    {
        RuleFor(command => command.Id)
            .NotEmpty();
    }
}
