using FluentValidation;
using TechChallenge.Application.Abstractions.Repositories;
using TechChallenge.Application.Validation;

namespace TechChallenge.Application.Features.Veiculos.AtualizarVeiculo;

public class AtualizarVeiculoService
{
    private readonly IVeiculoRepository _veiculoRepository;
    private readonly IClienteRepository _clienteRepository;
    private readonly IValidator<AtualizarVeiculoCommand> _validator;

    public AtualizarVeiculoService(
        IVeiculoRepository veiculoRepository,
        IClienteRepository clienteRepository,
        IValidator<AtualizarVeiculoCommand> validator)
    {
        _veiculoRepository = veiculoRepository;
        _clienteRepository = clienteRepository;
        _validator = validator;
    }

    public async Task<bool> AtualizarVeiculo(AtualizarVeiculoCommand command)
    {
        _validator.ValidateAndThrow(command);
        var placa = PlacaValidator.Formatar(command.Placa);

        var veiculo = await _veiculoRepository.GetByIdAsync(command.Id);
        if (veiculo is null)
            throw new KeyNotFoundException($"Veículo com Id {command.Id} não encontrado.");

        var placaExiste = await _veiculoRepository.GetByPlacaAsync(placa);
        if (placaExiste is not null && placaExiste.Id != command.Id)
            throw new InvalidOperationException($"Já existe outro veículo cadastrado com a placa {placa}.");

        var cliente = await _clienteRepository.GetByIdAsync(command.ClienteId);
        if (cliente is null)
            throw new KeyNotFoundException($"Cliente com Id {command.ClienteId} não encontrado.");

        veiculo.Atualizar(
            placa,
            command.Modelo,
            command.Marca,
            command.Cor,
            command.Ano,
            command.Quilometragem,
            command.Valor,
            command.ClienteId,
            command.CategoriaId);

        await _veiculoRepository.UpdateAsync(veiculo);
        return true;
    }
}
