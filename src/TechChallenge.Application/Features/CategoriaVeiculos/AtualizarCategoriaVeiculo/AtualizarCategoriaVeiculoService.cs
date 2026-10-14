using FluentValidation;
using TechChallenge.Application.Abstractions.Repositories;
using TechChallenge.Application.Validation;
using TechChallenge.Domain.Enums;

namespace TechChallenge.Application.Features.CategoriaVeiculos.AtualizarCategoriaVeiculo;

public class AtualizarCategoriaVeiculoService
{
    private readonly ICategoriaVeiculoRepository _categoriaVeiculoRepository;
    private readonly IValidator<AtualizarCategoriaVeiculoCommand> _validator;

    public AtualizarCategoriaVeiculoService(ICategoriaVeiculoRepository categoriaVeiculoRepository, IValidator<AtualizarCategoriaVeiculoCommand> validator)
    {
        _categoriaVeiculoRepository = categoriaVeiculoRepository;
        _validator = validator;
    }

    public async Task<bool> AtualizarCategoriaVeiculo(AtualizarCategoriaVeiculoCommand command)
    {
        _validator.ValidateAndThrow(command);

        var categoriaVeiculo = await _categoriaVeiculoRepository.GetByIdAsync(command.Id);

        if (categoriaVeiculo is null)
            throw new KeyNotFoundException($"Categoria de Veículo com Id {command.Id} não encontrada.");

        categoriaVeiculo = new Domain.Entities.CategoriaVeiculo(
            categoriaVeiculo.Id,
            command.Descricao
        );

        await _categoriaVeiculoRepository.UpdateAsync(categoriaVeiculo);
        return true;
    }
}