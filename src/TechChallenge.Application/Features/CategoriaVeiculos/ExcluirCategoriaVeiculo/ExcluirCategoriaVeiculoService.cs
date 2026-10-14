using FluentValidation;
using TechChallenge.Application.Abstractions.Repositories;

namespace TechChallenge.Application.Features.CategoriaVeiculos.ExcluirCategoriaVeiculo;

public class ExcluirCategoriaVeiculoService
{
    private readonly ICategoriaVeiculoRepository _categoriaVeiculoRepository;
    private readonly IVeiculoRepository _veiculoRepository;
    private readonly IValidator<ExcluirCategoriaVeiculoCommand> _validator;

    public ExcluirCategoriaVeiculoService(
        ICategoriaVeiculoRepository categoriaVeiculoRepository,
        IVeiculoRepository veiculoRepository,
        IValidator<ExcluirCategoriaVeiculoCommand> validator)
    {
        _categoriaVeiculoRepository = categoriaVeiculoRepository;
        _veiculoRepository = veiculoRepository;
        _validator = validator;
    }

    public async Task<bool> ExcluirCategoriaVeiculo(ExcluirCategoriaVeiculoCommand command)
    {
        _validator.ValidateAndThrow(command);

        var categoriaVeiculo = await _categoriaVeiculoRepository.GetByIdAsync(command.Id);
        if (categoriaVeiculo is null)
            throw new KeyNotFoundException($"Categoria de veículo com Id {command.Id} não encontrada.");

        var veiculosNaCategoria = await _veiculoRepository.ExisteVeiculoComCategoria(command.Id);

        if (veiculosNaCategoria)
            throw new InvalidOperationException("Não é possível excluir uma categoria de veículo que possui veículos associados.");

        await _categoriaVeiculoRepository.DeleteAsync(categoriaVeiculo);

        return true;
    }
}