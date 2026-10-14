namespace TechChallenge.Domain.Entities;

public class Endereco
{
    public Guid Id { get; private set; }
    public string Logradouro { get; private set; } = string.Empty;
    public string Complemento { get; private set; } = string.Empty;
    public string Numero { get; private set; } = string.Empty;
    public string Bairro { get; private set; } = string.Empty;
    public string Cidade { get; private set; } = string.Empty;
    public string Estado { get; private set; } = string.Empty;
    public string Cep { get; private set; } = string.Empty;

    public Endereco()
    {
        
    }

    public Endereco(Guid id, string logradouro, string complemento, string numero, string bairro, string cidade, string estado, string cep)
    {
        Id = id;
        Logradouro = logradouro;
        Complemento = complemento;
        Numero = numero;
        Bairro = bairro;
        Cidade = cidade;
        Estado = estado;
        Cep = cep;
    }

    public void Atualizar(string logradouro, string complemento, string numero, string bairro, string cidade, string estado, string cep)
    {
        Logradouro = logradouro;
        Complemento = complemento;
        Numero = numero;
        Bairro = bairro;
        Cidade = cidade;
        Estado = estado;
        Cep = cep;
    }
}
