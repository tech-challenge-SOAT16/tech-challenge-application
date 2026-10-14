using FluentValidation;
using TechChallenge.Application.Abstractions.Repositories;
using TechChallenge.Application.Validation;
using TechChallenge.Domain.Enums;

namespace TechChallenge.Application.Features.Clientes.AtualizarCliente;

public class AtualizarClienteService
{
    private readonly IClienteRepository _clienteRepository;
    private readonly IValidator<AtualizarClienteCommand> _validator;

    public AtualizarClienteService(IClienteRepository clienteRepository, IValidator<AtualizarClienteCommand> validator)
    {
        _clienteRepository = clienteRepository;
        _validator = validator;
    }

    public async Task<bool> AtualizarCliente(AtualizarClienteCommand command)
    {
        _validator.ValidateAndThrow(command);
        var documento = FormatarDocumento(command.TipoDocumento, command.Documento);

        var cliente = await _clienteRepository.GetByIdAsync(command.Id);
        if (cliente is null)
            throw new KeyNotFoundException($"Cliente com Id {command.Id} não encontrado.");

        cliente.Atualizar(command.Nome, command.Email.Trim(), command.TipoDocumento, documento);
        cliente.Endereco.Atualizar(
            command.Logradouro,
            command.Complemento,
            command.Numero,
            command.Bairro,
            command.Cidade,
            command.Estado,
            command.Cep);

        await _clienteRepository.UpdateAsync(cliente);
        return true;
    }

    private static string FormatarDocumento(TipoDocumento tipoDocumento, string documento) => tipoDocumento switch
    {
        TipoDocumento.Cpf => CpfValidator.Formatar(documento),
        TipoDocumento.Cnpj => CnpjValidator.Formatar(documento),
        _ => documento.Trim()
    };
}
