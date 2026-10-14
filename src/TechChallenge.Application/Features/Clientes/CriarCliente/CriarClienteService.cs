using System;
using FluentValidation;
using TechChallenge.Domain.Entities;
using TechChallenge.Domain.Enums;
using TechChallenge.Application.Abstractions.Repositories;
using TechChallenge.Application.Validation;

namespace TechChallenge.Application.Features.Clientes.CriarCliente;

public class CriarClienteService
{
    private readonly IClienteRepository _clienteRepository;
    private readonly IValidator<CriarClienteCommand> _validator;

    public CriarClienteService(IClienteRepository clienteRepository, IValidator<CriarClienteCommand> validator)
    {
        _clienteRepository = clienteRepository;
        _validator = validator;
    }

    public async Task<Guid> CriarCliente(CriarClienteCommand command)
    {
        _validator.ValidateAndThrow(command);
        var documento = FormatarDocumento(command.TipoDocumento, command.Documento);

        var clienteExiste = await _clienteRepository.GetByDocumentAsync(documento);
        if (clienteExiste is not null)
            throw new InvalidOperationException($"Já existe um cliente cadastrado com o {NomeDocumento(command.TipoDocumento)} {documento}.");

        Endereco endereco = new Endereco(Guid.NewGuid(), command.Logradouro, command.Complemento, command.Numero, command.Bairro, command.Cidade, command.Estado, command.Cep);

        var cliente = new Cliente(Guid.NewGuid(), command.Nome, command.Email.Trim(), command.TipoDocumento, documento, endereco.Id);
        cliente.AtribuirEndereco(endereco);

        await _clienteRepository.AddAsync(cliente);
        return cliente.Id;
    }

    private static string FormatarDocumento(TipoDocumento tipoDocumento, string documento) => tipoDocumento switch
    {
        TipoDocumento.Cpf => CpfValidator.Formatar(documento),
        TipoDocumento.Cnpj => CnpjValidator.Formatar(documento),
        _ => documento.Trim()
    };

    private static string NomeDocumento(TipoDocumento tipoDocumento) => tipoDocumento switch
    {
        TipoDocumento.Cpf => "CPF",
        TipoDocumento.Cnpj => "CNPJ",
        _ => "RG"
    };
}
