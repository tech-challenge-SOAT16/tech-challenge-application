using FluentValidation;
using TechChallenge.Application.Abstractions.Repositories;

namespace TechChallenge.Application.Features.CategoriaServicos.ExcluirCategoriaServico;

public class ExcluirCategoriaServicoService
{
    private readonly ICategoriaServicoRepository _categoriaServicoRepository;
    private readonly IServicoRepository _servicoRepository;
    private readonly IValidator<ExcluirCategoriaServicoCommand> _validator;

    public ExcluirCategoriaServicoService(
        ICategoriaServicoRepository categoriaServicoRepository,
        IServicoRepository servicoRepository,
        IValidator<ExcluirCategoriaServicoCommand> validator)
    {
        _categoriaServicoRepository = categoriaServicoRepository;
        _servicoRepository = servicoRepository;
        _validator = validator;
    }

    public async Task<bool> ExcluirCategoriaServico(ExcluirCategoriaServicoCommand command)
    {
        _validator.ValidateAndThrow(command);

        var categoriaServico = await _categoriaServicoRepository.GetByIdAsync(command.Id);

        if (categoriaServico is null)
            throw new KeyNotFoundException($"Categoria de serviço com Id {command.Id} não encontrada.");

        var servicoAssociado = await _servicoRepository.ExisteServicoComCategoria(command.Id);
        
        if (servicoAssociado)
            throw new InvalidOperationException("Não é possível excluir uma categoria de serviço associada a um serviço.");

        await _categoriaServicoRepository.DeleteAsync(categoriaServico);

        return true;
    }
}