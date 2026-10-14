using System;
using FluentValidation;
using TechChallenge.Application.Validation;

namespace TechChallenge.Application.Features.Veiculos.AtualizarVeiculo;

public class AtualizarVeiculoCommandValidator : AbstractValidator<AtualizarVeiculoCommand>
{
    public AtualizarVeiculoCommandValidator()
    {
        RuleFor(command => command.Id)
            .NotEmpty();

        RuleFor(command => command.Placa)
            .NotEmpty()
            .MaximumLength(10)
            .Must(PlacaValidator.PlacaValida)
            .WithMessage("Placa inválida.");

        RuleFor(command => command.Modelo)
            .NotEmpty()
            .MaximumLength(30);

        RuleFor(command => command.Marca)
            .NotEmpty()
            .MaximumLength(30);

        RuleFor(command => command.Cor)
            .NotEmpty()
            .MaximumLength(20);

        RuleFor(command => command.Ano)
            .GreaterThan(0)
            .LessThanOrEqualTo(DateTime.UtcNow.Year + 1);

        RuleFor(command => command.Quilometragem)
            .GreaterThanOrEqualTo(0);

        RuleFor(command => command.Valor)
            .GreaterThanOrEqualTo(0);

        RuleFor(command => command.ClienteId)
            .NotEmpty();

        RuleFor(command => command.CategoriaId)
            .NotEmpty();
    }
}
