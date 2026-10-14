using System;
using TechChallenge.Domain.Enums;

namespace TechChallenge.Domain.Entities;

public class Funcionario
{
    public Guid Id { get; private set; }
    public string Nome { get; private set; } = string.Empty;
    public string Cpf { get; private set; } = string.Empty;
    public string Rg { get; private set; } = string.Empty;
    public TipoFuncionario TipoFuncionario { get; private set; }
    public Guid EnderecoId { get; private set; }
    public Endereco Endereco { get; private set; } = null!;

    public Funcionario(Guid id, string nome, string cpf, string rg, TipoFuncionario tipoFuncionario, Guid enderecoId)
    {
        Id = id;
        Nome = nome;
        Cpf = cpf;
        Rg = rg;
        TipoFuncionario = tipoFuncionario;
        EnderecoId = enderecoId;
    }

    public void AtribuirEndereco(Endereco endereco)
    {
        Endereco = endereco;
        EnderecoId = endereco.Id;
    }

    public void Atualizar(string nome, string cpf, string rg, TipoFuncionario tipoFuncionario)
    {
        Nome = nome;
        Cpf = cpf;
        Rg = rg;
        TipoFuncionario = tipoFuncionario;
    }
}
