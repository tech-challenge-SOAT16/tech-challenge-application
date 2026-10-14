using FluentValidation;
using TechChallenge.Application.Abstractions.Repositories;

namespace TechChallenge.Application.Features.Estoque.BaixarEstoque;

public class BaixarEstoqueService
{
    private readonly IEstoqueRepository _estoqueRepository;
    private readonly IValidator<BaixarEstoqueCommand> _validator;

    public BaixarEstoqueService(IEstoqueRepository estoqueRepository, IValidator<BaixarEstoqueCommand> validator)
    {
        _estoqueRepository = estoqueRepository;
        _validator = validator;
    }

    public async Task<Domain.Entities.Estoque> BaixarEstoqueAsync(BaixarEstoqueCommand command)
    {
        _validator.ValidateAndThrow(command);

        var estoque = await _estoqueRepository.GetByIdProdutoAsync(command.ProdutoId);

        if (estoque is null)
            throw new KeyNotFoundException($"Estoque do produto {command.ProdutoId} não encontrado.");

        estoque.Baixar(command.Quantidade);

        return await _estoqueRepository.UpdateAsync(estoque);
    }
}
