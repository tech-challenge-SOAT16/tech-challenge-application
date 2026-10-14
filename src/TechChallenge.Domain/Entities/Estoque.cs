namespace TechChallenge.Domain.Entities;

public class Estoque
{
    public Guid Id { get; private set; }
    public Guid ProdutoId { get; private set; }
    public Produto Produto { get; private set; } = null!;
    public int Quantidade { get; private set; }
    public Guid Versao { get; private set; }

    private Estoque()
    {
    }

    public Estoque(Guid id, Guid produtoId, int quantidade)
    {
        if (id == Guid.Empty)
            throw new ArgumentException("O identificador do estoque é obrigatório.", nameof(id));

        if (produtoId == Guid.Empty)
            throw new ArgumentException("O identificador do produto é obrigatório.", nameof(produtoId));

        if (quantidade < 0)
            throw new ArgumentOutOfRangeException(nameof(quantidade), "A quantidade do estoque não pode ser negativa.");

        Id = id;
        ProdutoId = produtoId;
        Quantidade = quantidade;
        Versao = Guid.NewGuid();
    }

    public void Adicionar(int quantidade)
    {
        if (quantidade <= 0)
            throw new ArgumentOutOfRangeException(nameof(quantidade), "A quantidade adicionada deve ser maior que zero.");

        Quantidade = checked(Quantidade + quantidade);
        AtualizarVersao();
    }

    public void Baixar(int quantidade)
    {
        if (quantidade <= 0)
            throw new ArgumentOutOfRangeException(nameof(quantidade), "A quantidade baixada deve ser maior que zero.");

        if (quantidade > Quantidade)
            throw new InvalidOperationException(
                $"Estoque insuficiente. Disponível: {Quantidade}; solicitado: {quantidade}.");

        Quantidade -= quantidade;
        AtualizarVersao();
    }

    public void DefinirQuantidade(int quantidade)
    {
        if (quantidade < 0)
            throw new ArgumentOutOfRangeException(nameof(quantidade), "A quantidade do estoque não pode ser negativa.");

        Quantidade = quantidade;
        AtualizarVersao();
    }

    private void AtualizarVersao() => Versao = Guid.NewGuid();
}
