using System;
using FluentAssertions;
using TechChallenge.Application.Features.Veiculos.CriarVeiculo;
using TechChallenge.Domain.Enums;

namespace TechChallenge.Tests.Features.Veiculos.CriarVeiculo;

public class CriarVeiculoCommandValidatorTests
{
    private readonly CriarVeiculoCommandValidator _validator = new();

    [Fact]
    public void DeveValidarCommandQuandoDadosDoVeiculoEstaoCorretos()
    {
        var command = CriarCommandValido();

        var resultado = _validator.Validate(command);

        resultado.IsValid.Should().BeTrue();
    }

    [Theory]
    [InlineData("ABC1234")]
    [InlineData("ABC-1234")]
    [InlineData("abc1234")]
    [InlineData("ABC1D23")]
    [InlineData("abc1d23")]
    public void DeveValidarPlacaAntigaEMercosul(string placa)
    {
        var command = CriarCommandValido();
        command.Placa = placa;

        var resultado = _validator.Validate(command);

        resultado.IsValid.Should().BeTrue();
    }

    [Theory]
    [InlineData("AB12345")]
    [InlineData("ABCD123")]
    [InlineData("ABC12D3")]
    [InlineData("ABC123")]
    public void DeveRetornarErroQuandoPlacaForInvalida(string placa)
    {
        var command = CriarCommandValido();
        command.Placa = placa;

        var resultado = _validator.Validate(command);

        resultado.IsValid.Should().BeFalse();
        resultado.Errors.Should().Contain(error =>
            error.PropertyName == nameof(CriarVeiculoCommand.Placa) &&
            error.ErrorMessage == "Placa inválida.");
    }

    [Fact]
    public void DeveRetornarErroQuandoClienteIdEstiverVazio()
    {
        var command = CriarCommandValido();
        command.ClienteId = Guid.Empty;

        var resultado = _validator.Validate(command);

        resultado.IsValid.Should().BeFalse();
        resultado.Errors.Should().Contain(error => error.PropertyName == nameof(CriarVeiculoCommand.ClienteId));
    }

    [Fact]
    public void DeveRetornarErroQuandoAnoOuValoresNumericosForemInvalidos()
    {
        var command = CriarCommandValido();
        command.Ano = 0;
        command.Quilometragem = -1;
        command.Valor = -1;

        var resultado = _validator.Validate(command);

        resultado.IsValid.Should().BeFalse();
        resultado.Errors.Select(error => error.PropertyName).Should().Contain([
            nameof(CriarVeiculoCommand.Ano),
            nameof(CriarVeiculoCommand.Quilometragem),
            nameof(CriarVeiculoCommand.Valor)
        ]);
    }

    private static CriarVeiculoCommand CriarCommandValido()
    {
        return new CriarVeiculoCommand
        {
            Placa = "ABC-1234",
            Modelo = "Civic",
            Marca = "Honda",
            Cor = "Prata",
            Ano = 2022,
            Quilometragem = 10000,
            Valor = 90000,
            ClienteId = Guid.NewGuid(),
            CategoriaId = Guid.NewGuid()
        };
    }
}
