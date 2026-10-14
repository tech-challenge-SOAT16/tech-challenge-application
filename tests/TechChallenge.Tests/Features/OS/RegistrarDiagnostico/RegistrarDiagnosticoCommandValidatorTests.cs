using System;
using FluentAssertions;
using TechChallenge.Application.Features.OS.RegistrarDiagnostico;

namespace TechChallenge.Tests.Features.OS.RegistrarDiagnostico;

public class RegistrarDiagnosticoCommandValidatorTests
{
    private readonly RegistrarDiagnosticoCommandValidator _validator = new();

    [Fact]
    public void DeveValidarCommandQuandoServicoOuProdutoForInformado()
    {
        var command = new RegistrarDiagnosticoCommand
        {
            OrdemServicoId = Guid.NewGuid(),
            Servicos = [new ItemDiagnosticoCommand { Id = Guid.NewGuid(), Quantidade = 1 }]
        };

        var resultado = _validator.Validate(command);

        resultado.IsValid.Should().BeTrue();
    }

    [Fact]
    public void DeveRetornarErroQuandoNaoHouverServicoNemProduto()
    {
        var command = new RegistrarDiagnosticoCommand
        {
            OrdemServicoId = Guid.NewGuid()
        };

        var resultado = _validator.Validate(command);

        resultado.IsValid.Should().BeFalse();
        resultado.Errors.Should().Contain(error =>
            error.ErrorMessage == "Informe ao menos um serviço ou produto para registrar o diagnóstico.");
    }

    [Fact]
    public void DeveRetornarErroQuandoOrdemServicoIdOuItensEstiveremVazios()
    {
        var command = new RegistrarDiagnosticoCommand
        {
            OrdemServicoId = Guid.Empty,
            Servicos = [new ItemDiagnosticoCommand { Quantidade = 0 }],
            Produtos = [new ItemDiagnosticoCommand { Quantidade = 0 }]
        };

        var resultado = _validator.Validate(command);

        resultado.IsValid.Should().BeFalse();
        resultado.Errors.Select(error => error.PropertyName).Should().Contain([
            nameof(RegistrarDiagnosticoCommand.OrdemServicoId),
            $"{nameof(RegistrarDiagnosticoCommand.Servicos)}[0].{nameof(ItemDiagnosticoCommand.Id)}",
            $"{nameof(RegistrarDiagnosticoCommand.Servicos)}[0].{nameof(ItemDiagnosticoCommand.Quantidade)}",
            $"{nameof(RegistrarDiagnosticoCommand.Produtos)}[0].{nameof(ItemDiagnosticoCommand.Id)}",
            $"{nameof(RegistrarDiagnosticoCommand.Produtos)}[0].{nameof(ItemDiagnosticoCommand.Quantidade)}"
        ]);
    }
}
