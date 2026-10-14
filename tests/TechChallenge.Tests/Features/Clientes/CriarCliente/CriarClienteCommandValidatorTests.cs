using FluentAssertions;
using TechChallenge.Application.Features.Clientes.CriarCliente;
using TechChallenge.Domain.Enums;

namespace TechChallenge.Tests.Features.Clientes.CriarCliente;

public class CriarClienteCommandValidatorTests
{
    private readonly CriarClienteCommandValidator _validator = new();

    [Fact]
    public void DeveValidarCommandQuandoDadosObrigatoriosEstaoCorretos()
    {
        var command = CriarCommandValido();

        var resultado = _validator.Validate(command);

        resultado.IsValid.Should().BeTrue();
    }

    [Fact]
    public void DeveRetornarErroQuandoCpfForInvalido()
    {
        var command = CriarCommandValido();
        command.Documento = "12345678901";

        var resultado = _validator.Validate(command);

        resultado.IsValid.Should().BeFalse();
        resultado.Errors.Should().Contain(error =>
            error.PropertyName == nameof(CriarClienteCommand.Documento) &&
            error.ErrorMessage == "CPF inválido.");
    }

    [Fact]
    public void DeveValidarCommandQuandoCnpjForInformado()
    {
        var command = CriarCommandValido();
        command.TipoDocumento = TipoDocumento.Cnpj;
        command.Documento = "11222333000181";

        var resultado = _validator.Validate(command);

        resultado.IsValid.Should().BeTrue();
    }

    [Fact]
    public void DeveRetornarErroQuandoDocumentoNaoForInformado()
    {
        var command = CriarCommandValido();
        command.Documento = string.Empty;

        var resultado = _validator.Validate(command);

        resultado.IsValid.Should().BeFalse();
        resultado.Errors.Should().Contain(error => error.PropertyName == nameof(CriarClienteCommand.Documento));
    }

    [Fact]
    public void DeveRetornarErroQuandoCamposObrigatoriosEstiveremVazios()
    {
        var command = CriarCommandValido();
        command.Nome = string.Empty;
        command.Logradouro = string.Empty;
        command.Numero = string.Empty;
        command.Bairro = string.Empty;
        command.Cidade = string.Empty;
        command.Estado = string.Empty;
        command.Cep = string.Empty;

        var resultado = _validator.Validate(command);

        resultado.IsValid.Should().BeFalse();
        resultado.Errors.Select(error => error.PropertyName).Should().Contain([
            nameof(CriarClienteCommand.Nome),
            nameof(CriarClienteCommand.Logradouro),
            nameof(CriarClienteCommand.Numero),
            nameof(CriarClienteCommand.Bairro),
            nameof(CriarClienteCommand.Cidade),
            nameof(CriarClienteCommand.Estado),
            nameof(CriarClienteCommand.Cep)
        ]);
    }

    private static CriarClienteCommand CriarCommandValido()
    {
        return new CriarClienteCommand
        {
            Nome = "Maria Cliente",
            Email = "maria.cliente@teste.local",
            TipoDocumento = TipoDocumento.Cpf,
            Documento = "52998224725",
            Logradouro = "Rua Teste",
            Complemento = "Apto 10",
            Numero = "100",
            Bairro = "Centro",
            Cidade = "Sao Paulo",
            Estado = "SP",
            Cep = "01001000"
        };
    }
}
