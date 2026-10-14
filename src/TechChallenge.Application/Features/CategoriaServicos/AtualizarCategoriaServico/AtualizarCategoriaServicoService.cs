using FluentValidation;
using TechChallenge.Application.Abstractions.Repositories;

namespace TechChallenge.Application.Features.CategoriaServicos.AtualizarCategoriaServico;

public class AtualizarCategoriaServicoService
{
    private readonly ICategoriaServicoRepository _categoriaServicoRepository;
    private readonly IValidator<AtualizarCategoriaServicoCommand> _validator;

    public AtualizarCategoriaServicoService(ICategoriaServicoRepository categoriaServicoRepository, IValidator<AtualizarCategoriaServicoCommand> validator)
    {
        _categoriaServicoRepository = categoriaServicoRepository;
        _validator = validator;
    }

    public async Task<bool> AtualizarCategoriaServico(AtualizarCategoriaServicoCommand command)
    {
        _validator.ValidateAndThrow(command);

        var categoriaServico = await _categoriaServicoRepository.GetByIdAsync(command.Id);

        if (categoriaServico is null)
            throw new KeyNotFoundException($"Categoria de Serviço com Id {command.Id} não encontrada.");

        categoriaServico = new Domain.Entities.CategoriaServico(
            categoriaServico.Id,
            command.Descricao
        );

        await _categoriaServicoRepository.UpdateAsync(categoriaServico);
        
        return true;
    }
}