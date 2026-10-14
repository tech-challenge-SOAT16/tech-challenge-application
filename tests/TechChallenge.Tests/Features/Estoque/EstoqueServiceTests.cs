using FluentAssertions;
using Moq;
using TechChallenge.Application.Abstractions.Repositories;
using TechChallenge.Application.Features.Estoque.AdicionarEstoque;
using TechChallenge.Application.Features.Estoque.BaixarEstoque;
using TechChallenge.Domain.Entities;

namespace TechChallenge.Tests.Features.Estoque;

public class EstoqueServiceTests
{
    [Fact]
    public async Task DeveCriarEstoqueComQuantidadeInformada()
    {
        var produto = new Produto(Guid.NewGuid(), "Filtro", 50, Guid.NewGuid());
        var estoqueRepository = new Mock<IEstoqueRepository>();
        var produtoRepository = new Mock<IProdutoRepository>();
        produtoRepository.Setup(repo => repo.GetByIdAsync(produto.Id)).ReturnsAsync(produto);
        estoqueRepository.Setup(repo => repo.AddAsync(It.IsAny<TechChallenge.Domain.Entities.Estoque>()))
            .ReturnsAsync((TechChallenge.Domain.Entities.Estoque estoque) => estoque);
        var service = new AdicionarEstoqueService(
            estoqueRepository.Object, produtoRepository.Object, new AdicionarEstoqueCommandValidator());

        var resultado = await service.AdicionarEstoqueAsync(new AdicionarEstoqueCommand
        {
            ProdutoId = produto.Id,
            Quantidade = 8
        });

        resultado.Quantidade.Should().Be(8);
        estoqueRepository.Verify(repo => repo.AddAsync(
            It.Is<TechChallenge.Domain.Entities.Estoque>(estoque => estoque.ProdutoId == produto.Id && estoque.Quantidade == 8)),
            Times.Once);
    }

    [Fact]
    public async Task DeveSomarQuantidadeAoEstoqueExistente()
    {
        var produto = new Produto(Guid.NewGuid(), "Filtro", 50, Guid.NewGuid());
        var estoque = new TechChallenge.Domain.Entities.Estoque(Guid.NewGuid(), produto.Id, 4);
        var estoqueRepository = new Mock<IEstoqueRepository>();
        var produtoRepository = new Mock<IProdutoRepository>();
        produtoRepository.Setup(repo => repo.GetByIdAsync(produto.Id)).ReturnsAsync(produto);
        estoqueRepository.Setup(repo => repo.GetByIdProdutoAsync(produto.Id)).ReturnsAsync(estoque);
        estoqueRepository.Setup(repo => repo.UpdateAsync(estoque)).ReturnsAsync(estoque);
        var service = new AdicionarEstoqueService(
            estoqueRepository.Object, produtoRepository.Object, new AdicionarEstoqueCommandValidator());

        var resultado = await service.AdicionarEstoqueAsync(new AdicionarEstoqueCommand
        {
            ProdutoId = produto.Id,
            Quantidade = 3
        });

        resultado.Quantidade.Should().Be(7);
        estoqueRepository.Verify(repo => repo.UpdateAsync(estoque), Times.Once);
    }

    [Fact]
    public async Task DeveBaixarEstoquePeloIdDoProduto()
    {
        var produtoId = Guid.NewGuid();
        var estoque = new TechChallenge.Domain.Entities.Estoque(Guid.NewGuid(), produtoId, 10);
        var repository = new Mock<IEstoqueRepository>();
        repository.Setup(repo => repo.GetByIdProdutoAsync(produtoId)).ReturnsAsync(estoque);
        repository.Setup(repo => repo.UpdateAsync(estoque)).ReturnsAsync(estoque);
        var service = new BaixarEstoqueService(repository.Object, new BaixarEstoqueCommandValidator());

        var resultado = await service.BaixarEstoqueAsync(new BaixarEstoqueCommand
        {
            ProdutoId = produtoId,
            Quantidade = 6
        });

        resultado.Quantidade.Should().Be(4);
        repository.Verify(repo => repo.GetByIdProdutoAsync(produtoId), Times.Once);
        repository.Verify(repo => repo.GetByIdAsync(It.IsAny<Guid>()), Times.Never);
    }

    [Fact]
    public async Task DeveImpedirBaixaMaiorQueOSaldo()
    {
        var produtoId = Guid.NewGuid();
        var estoque = new TechChallenge.Domain.Entities.Estoque(Guid.NewGuid(), produtoId, 2);
        var repository = new Mock<IEstoqueRepository>();
        repository.Setup(repo => repo.GetByIdProdutoAsync(produtoId)).ReturnsAsync(estoque);
        var service = new BaixarEstoqueService(repository.Object, new BaixarEstoqueCommandValidator());

        var act = () => service.BaixarEstoqueAsync(new BaixarEstoqueCommand
        {
            ProdutoId = produtoId,
            Quantidade = 3
        });

        await act.Should().ThrowAsync<InvalidOperationException>();
        repository.Verify(repo => repo.UpdateAsync(It.IsAny<TechChallenge.Domain.Entities.Estoque>()), Times.Never);
    }
}
