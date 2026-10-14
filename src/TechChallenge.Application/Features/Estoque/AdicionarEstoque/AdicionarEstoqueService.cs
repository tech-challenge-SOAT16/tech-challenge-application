using FluentValidation;
using TechChallenge.Application.Abstractions.Repositories;

namespace TechChallenge.Application.Features.Estoque.AdicionarEstoque;

public class AdicionarEstoqueService
{
    private readonly IEstoqueRepository _estoqueRepository;
    private readonly IProdutoRepository _produtoRepository;
    private readonly IValidator<AdicionarEstoqueCommand> _validator;

    public AdicionarEstoqueService(IEstoqueRepository estoqueRepository, IProdutoRepository produtoRepository, IValidator<AdicionarEstoqueCommand> validator)
    {
        _estoqueRepository = estoqueRepository;
        _produtoRepository = produtoRepository;
        _validator = validator;
    }

    public async Task<Domain.Entities.Estoque> AdicionarEstoqueAsync(AdicionarEstoqueCommand command)
    {
        _validator.ValidateAndThrow(command);

        var produto = await _produtoRepository.GetByIdAsync(command.ProdutoId);

        if (produto is null)
            throw new KeyNotFoundException($"Produto {command.ProdutoId} não encontrado");

        var estoque = await _estoqueRepository.GetByIdProdutoAsync(command.ProdutoId);
        
        if (estoque is null)
            return await _estoqueRepository.AddAsync(
                new Domain.Entities.Estoque(Guid.NewGuid(), command.ProdutoId, command.Quantidade));
        else
        {
            estoque.Adicionar(command.Quantidade);
            return await _estoqueRepository.UpdateAsync(estoque);
        }
    }
}
