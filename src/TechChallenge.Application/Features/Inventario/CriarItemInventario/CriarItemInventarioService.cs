using System;
using FluentValidation;
using TechChallenge.Domain.Entities;
using TechChallenge.Application.Abstractions.Repositories;

namespace TechChallenge.Application.Features.Inventario.CriarItemInventario;

public class CriarItemInventarioService
{
    private readonly IProdutoRepository _produtoRepository;
    private readonly ICategoriaProdutoRepository _categoriaProdutoRepository;
    private readonly IEstoqueRepository _estoqueRepository;
    private readonly IValidator<CriarItemInventarioCommand> _validator;

    public CriarItemInventarioService(
        IProdutoRepository produtoRepository,
        ICategoriaProdutoRepository categoriaProdutoRepository,
        IEstoqueRepository estoqueRepository,
        IValidator<CriarItemInventarioCommand> validator)
    {
        _produtoRepository = produtoRepository;
        _categoriaProdutoRepository = categoriaProdutoRepository;
        _estoqueRepository = estoqueRepository;
        _validator = validator;
    }

    public async Task<Guid> CriarItemInventarioAsync(CriarItemInventarioCommand command)
    {
        _validator.ValidateAndThrow(command);

        var categoria = await _categoriaProdutoRepository.GetByIdAsync(command.IdCategoria);
        if (categoria is null)
            throw new KeyNotFoundException($"Categoria de produto com Id {command.IdCategoria} não encontrada.");

        var produto = new Produto(Guid.NewGuid(), command.Descricao, command.Valor, command.IdCategoria);

        await _produtoRepository.AddAsync(produto);
        await _estoqueRepository.AddAsync(
            new TechChallenge.Domain.Entities.Estoque(Guid.NewGuid(), produto.Id, command.Quantidade));
        return produto.Id;
    }
}
