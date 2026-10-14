using FluentValidation;
using TechChallenge.Application.Validation;
using TechChallenge.Domain.Enums;

namespace TechChallenge.Application.Features.Clientes.CriarCliente;

public class CriarClienteCommandValidator : AbstractValidator<CriarClienteCommand>
{
    public CriarClienteCommandValidator()
    {
        RuleFor(command => command.Nome)
            .NotEmpty()
            .MaximumLength(100);

        RuleFor(command => command.Email)
            .NotEmpty()
            .EmailAddress()
            .MaximumLength(254);

        RuleFor(command => command.TipoDocumento)
            .IsInEnum();

        RuleFor(command => command.Documento)
            .NotEmpty()
            .MaximumLength(18);

        RuleFor(command => command.Documento)
            .Must(CpfValidator.CpfValido)
            .WithMessage("CPF inválido.")
            .When(command => command.TipoDocumento == TipoDocumento.Cpf);

        RuleFor(command => command.Documento)
            .Must(CnpjValidator.CnpjValido)
            .WithMessage("CNPJ inválido.")
            .When(command => command.TipoDocumento == TipoDocumento.Cnpj);

        RuleFor(command => command.Documento)
            .MaximumLength(12)
            .When(command => command.TipoDocumento == TipoDocumento.Rg);

        RuleFor(command => command.Logradouro)
            .NotEmpty()
            .MaximumLength(100);

        RuleFor(command => command.Complemento)
            .MaximumLength(80);

        RuleFor(command => command.Numero)
            .NotEmpty()
            .MaximumLength(10);

        RuleFor(command => command.Bairro)
            .NotEmpty()
            .MaximumLength(50);

        RuleFor(command => command.Cidade)
            .NotEmpty()
            .MaximumLength(50);

        RuleFor(command => command.Estado)
            .NotEmpty()
            .MaximumLength(30);

        RuleFor(command => command.Cep)
            .NotEmpty()
            .MaximumLength(30);
    }
}
