using System;
using FluentValidation;
using TechChallenge.Domain.Entities;
using TechChallenge.Domain.Enums;
using TechChallenge.Application.Abstractions.Repositories;
using TechChallenge.Application.Validation;

namespace TechChallenge.Application.Features.Funcionarios.CriarFuncionario;

public class CriarFuncionarioService
{
    private readonly IFuncionarioRepository _funcionarioRepository;
    private readonly IValidator<CriarFuncionarioCommand> _validator;

    public CriarFuncionarioService(IFuncionarioRepository funcionarioRepository, IValidator<CriarFuncionarioCommand> validator)
    {
        _funcionarioRepository = funcionarioRepository;
        _validator = validator;
    }

    public async Task<Guid> CriarFuncionario(CriarFuncionarioCommand command)
    {
        _validator.ValidateAndThrow(command);
        var cpf = CpfValidator.Formatar(command.Cpf);

        var funcionarioExiste = await _funcionarioRepository.GetByDocumentAsync(cpf);
        if (funcionarioExiste is not null)
            throw new InvalidOperationException($"Já existe um funcionário cadastrado com o CPF {cpf}.");

        if (!Enum.TryParse<TipoFuncionario>(command.Cargo, true, out var tipoFuncionario))
            throw new InvalidOperationException($"Cargo {command.Cargo} inválido.");

        Endereco endereco = new Endereco(Guid.NewGuid(), command.Logradouro, command.Complemento, command.Numero, command.Bairro, command.Cidade, command.Estado, command.Cep);

        var funcionario = new Funcionario(Guid.NewGuid(), command.Nome, cpf, command.Rg, tipoFuncionario, endereco.Id);
        funcionario.AtribuirEndereco(endereco);

        await _funcionarioRepository.AddAsync(funcionario);
        return funcionario.Id;
    }
}
