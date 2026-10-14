namespace TechChallenge.Domain.Entities;

public class Veiculo
{
    public Guid Id { get; private set; }
    public string Placa { get; private set; } = string.Empty;
    public string Modelo { get; private set; } = string.Empty;
    public string Marca { get; private set; } = string.Empty;
    public string Cor { get; private set; } = string.Empty;
    public int Ano { get; private set; }
    public decimal Quilometragem { get; private set; }
    public decimal Valor { get; private set; }
    public Guid ClienteId { get; private set; }
    public Cliente ClienteResponsavel { get; private set; } = null!;
    public Guid CategoriaId { get; private set; }
    public CategoriaVeiculo Categoria { get; private set; } = null!;

    public Veiculo(Guid id, string placa, string modelo, string marca, string cor, int ano, decimal quilometragem, decimal valor, Guid clienteId, Guid categoriaId)
    {
        Id = id;
        Placa = placa;
        Modelo = modelo;
        Marca = marca;
        Cor = cor;
        Ano = ano;
        Quilometragem = quilometragem;
        Valor = valor;
        ClienteId = clienteId;
        CategoriaId = categoriaId;
    }

    public void Atualizar(string placa, string modelo, string marca, string cor, int ano, decimal quilometragem, decimal valor, Guid clienteId, Guid categoriaId)
    {
        Placa = placa;
        Modelo = modelo;
        Marca = marca;
        Cor = cor;
        Ano = ano;
        Quilometragem = quilometragem;
        Valor = valor;
        ClienteId = clienteId;
        CategoriaId = categoriaId;
    }
}
