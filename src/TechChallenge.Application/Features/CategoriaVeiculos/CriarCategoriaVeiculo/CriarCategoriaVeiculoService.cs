using FluentValidation;
using TechChallenge.Application.Abstractions.Repositories;

namespace TechChallenge.Application.Features.CategoriaVeiculos.CriarCategoriaVeiculo;

public class CriarCategoriaVeiculoService
{
    private readonly ICategoriaVeiculoRepository _categoriaVeiculoRepository;
    private readonly IValidator<CriarCategoriaVeiculoCommand> _validator;

    public CriarCategoriaVeiculoService(ICategoriaVeiculoRepository categoriaVeiculoRepository, IValidator<CriarCategoriaVeiculoCommand> validator)
    {
        _categoriaVeiculoRepository = categoriaVeiculoRepository;
        _validator = validator;
    }

    public async Task<Guid> CriarCategoriaVeiculo(CriarCategoriaVeiculoCommand command)
    {
        _validator.ValidateAndThrow(command);
        var descricao = command.Descricao.Trim();

        var categoriaVeiculoExiste = await _categoriaVeiculoRepository.GetByDescricaoAsync(descricao);
        if (categoriaVeiculoExiste is not null)
            throw new InvalidOperationException($"Já existe uma categoria de veículo cadastrada com a descrição {descricao}.");

        var categoriaVeiculo = new Domain.Entities.CategoriaVeiculo(Guid.NewGuid(), command.Descricao);

        await _categoriaVeiculoRepository.AddAsync(categoriaVeiculo);

        return categoriaVeiculo.Id;
    }
}