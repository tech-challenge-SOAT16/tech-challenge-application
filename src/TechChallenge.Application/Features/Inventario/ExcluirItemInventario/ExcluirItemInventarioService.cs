using FluentValidation;
using TechChallenge.Application.Abstractions.Repositories;

namespace TechChallenge.Application.Features.Inventario.ExcluirItemInventario;

public class ExcluirItemInventarioService
{
    private readonly IProdutoRepository _produtoRepository;
    private readonly IOrdemServicoProdutosRepository _ordemServicoProdutosRepository;
    private readonly IEstoqueRepository _estoqueRepository;
    private readonly IValidator<ExcluirItemInventarioCommand> _validator;

    public ExcluirItemInventarioService(
        IProdutoRepository produtoRepository,
        IOrdemServicoProdutosRepository ordemServicoProdutosRepository,
        IEstoqueRepository estoqueRepository,
        IValidator<ExcluirItemInventarioCommand> validator)
    {
        _produtoRepository = produtoRepository;
        _ordemServicoProdutosRepository = ordemServicoProdutosRepository;
        _estoqueRepository = estoqueRepository;
        _validator = validator;
    }

    public async Task<bool> ExcluirItemInventarioAsync(ExcluirItemInventarioCommand command)
    {
        _validator.ValidateAndThrow(command);

        var produto = await _produtoRepository.GetByIdAsync(command.Id);
        if (produto is null)
            throw new KeyNotFoundException($"Item de inventário com Id {command.Id} não encontrado.");

        var produtoEmUso = await _ordemServicoProdutosRepository.ExisteProdutoEmOrdemServicoAsync(command.Id);
        if (produtoEmUso)
            throw new InvalidOperationException("Não é possível excluir um produto associado a uma ordem de serviço.");

        var estoque = await _estoqueRepository.GetByIdProdutoAsync(command.Id);
        if (estoque is not null)
            await _estoqueRepository.DeleteAsync(estoque);

        await _produtoRepository.DeleteAsync(produto);
        return true;
    }
}
