using FluentValidation;
using TechChallenge.Application.Abstractions.Repositories;

namespace TechChallenge.Application.Features.Inventario.AtualizarItemInventario;

public class AtualizarItemInventarioService
{
    private readonly IProdutoRepository _produtoRepository;
    private readonly IEstoqueRepository _estoqueRepository;
    private readonly IValidator<AtualizarItemInventarioCommand> _validator;

    public AtualizarItemInventarioService(
        IProdutoRepository produtoRepository,
        IEstoqueRepository estoqueRepository,
        IValidator<AtualizarItemInventarioCommand> validator)
    {
        _produtoRepository = produtoRepository;
        _estoqueRepository = estoqueRepository;
        _validator = validator;
    }

    public async Task<bool> AtualizarItemInventarioAsync(AtualizarItemInventarioCommand command)
    {
        _validator.ValidateAndThrow(command);

        var produto = await _produtoRepository.GetByIdAsync(command.Id);
        if (produto is null)
            throw new KeyNotFoundException($"Item de inventário com Id {command.Id} não encontrado.");

        produto.Atualizar(command.Descricao, command.Valor);

        var estoque = await _estoqueRepository.GetByIdProdutoAsync(command.Id);

        if (estoque is null)
            throw new KeyNotFoundException($"Estoque para o item de inventário com Id {command.Id} não encontrado.");

        estoque.DefinirQuantidade(command.Quantidade);

        await _produtoRepository.UpdateAsync(produto);
        await _estoqueRepository.UpdateAsync(estoque);
        return true;
    }
}
