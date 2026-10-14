using System;
using FluentValidation;
using TechChallenge.Domain.Entities;
using TechChallenge.Application.Abstractions.Repositories;

namespace TechChallenge.Application.Features.Servicos.CriarServico;

public class CriarServicoService
{
    private readonly IServicoRepository _servicoRepository;
    private readonly ICategoriaServicoRepository _categoriaServicoRepository;
    private readonly IValidator<CriarServicoCommand> _validator;

    public CriarServicoService(
        IServicoRepository servicoRepository,
        ICategoriaServicoRepository categoriaServicoRepository,
        IValidator<CriarServicoCommand> validator)
    {
        _servicoRepository = servicoRepository;
        _categoriaServicoRepository = categoriaServicoRepository;
        _validator = validator;
    }

    public async Task<Guid> CriarServico(CriarServicoCommand command)
    {
        _validator.ValidateAndThrow(command);

        var categoria = await _categoriaServicoRepository.GetByIdAsync(command.CategoriaId);
        if (categoria is null)
            throw new KeyNotFoundException($"Categoria de serviço com Id {command.CategoriaId} não encontrada.");

        var servico = new Servico(Guid.NewGuid(), command.Descricao, command.Valor, command.CategoriaId);

        await _servicoRepository.AddAsync(servico);
        return servico.Id;
    }
}
