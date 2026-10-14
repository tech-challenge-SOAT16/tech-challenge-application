namespace TechChallenge.Domain.Entities;

public class Servico
{
    public Guid Id { get;private set; }
    public string Descricao { get;private set; } = string.Empty;
    public decimal Valor { get;private set; }
    public Guid CategoriaId { get; private set; }
    public CategoriaServico Categoria { get; private set; } = null!;

    public Servico(Guid id, string descricao, decimal valor, Guid categoriaId)
    {
        Id = id;
        Descricao = descricao;
        Valor = valor;
        CategoriaId = categoriaId;
    }

    public void Atualizar(string descricao, decimal valor)
    {
        Descricao = descricao;
        Valor = valor;
    }
}
