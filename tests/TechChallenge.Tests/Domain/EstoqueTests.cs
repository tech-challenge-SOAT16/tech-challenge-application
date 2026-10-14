using FluentAssertions;
using TechChallenge.Domain.Entities;

namespace TechChallenge.Tests.Domain;

public class EstoqueTests
{
    [Fact]
    public void DeveAdicionarQuantidadeEAtualizarVersao()
    {
        var estoque = new Estoque(Guid.NewGuid(), Guid.NewGuid(), 10);
        var versaoInicial = estoque.Versao;

        estoque.Adicionar(5);

        estoque.Quantidade.Should().Be(15);
        estoque.Versao.Should().NotBe(versaoInicial);
    }

    [Fact]
    public void DeveBaixarQuantidadeDisponivel()
    {
        var estoque = new Estoque(Guid.NewGuid(), Guid.NewGuid(), 10);

        estoque.Baixar(4);

        estoque.Quantidade.Should().Be(6);
    }

    [Fact]
    public void DeveImpedirSaldoNegativo()
    {
        var estoque = new Estoque(Guid.NewGuid(), Guid.NewGuid(), 2);

        var act = () => estoque.Baixar(3);

        act.Should().Throw<InvalidOperationException>()
            .WithMessage("Estoque insuficiente*");
        estoque.Quantidade.Should().Be(2);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void DeveRejeitarMovimentacaoNaoPositiva(int quantidade)
    {
        var estoque = new Estoque(Guid.NewGuid(), Guid.NewGuid(), 2);

        var adicionar = () => estoque.Adicionar(quantidade);
        var baixar = () => estoque.Baixar(quantidade);

        adicionar.Should().Throw<ArgumentOutOfRangeException>();
        baixar.Should().Throw<ArgumentOutOfRangeException>();
    }
}
