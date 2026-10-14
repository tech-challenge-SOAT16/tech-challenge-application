using System;
using FluentAssertions;
using Moq;
using TechChallenge.Application.Abstractions.Repositories;
using TechChallenge.Application.Features.Clientes.CriarCliente;
using TechChallenge.Domain.Entities;
using TechChallenge.Domain.Enums;

namespace TechChallenge.Tests.Features.Clientes.CriarCliente;

public class CriarClienteServiceTests
{
    [Fact]
    public async Task DeveCriarClienteQuandoCommandForValidoECpfNaoExistir()
    {
        var command = CriarCommandValido();
        const string cpfFormatado = "529.982.247-25";
        Cliente? clienteCriado = null;
        var repository = new Mock<IClienteRepository>();
        repository.Setup(repo => repo.GetByDocumentAsync(cpfFormatado))
            .ReturnsAsync((Cliente?)null);
        repository.Setup(repo => repo.AddAsync(It.IsAny<Cliente>()))
            .Callback<Cliente>(cliente => clienteCriado = cliente)
            .Returns<Cliente>(cliente => Task.FromResult(cliente));
        var service = new CriarClienteService(repository.Object, new CriarClienteCommandValidator());

        var clienteId = await service.CriarCliente(command);

        clienteId.Should().NotBeEmpty();
        clienteCriado.Should().NotBeNull();
        clienteCriado.Id.Should().Be(clienteId);
        clienteCriado.Nome.Should().Be(command.Nome);
        clienteCriado.Email.Should().Be(command.Email);
        clienteCriado.TipoDocumento.Should().Be(TipoDocumento.Cpf);
        clienteCriado.Documento.Should().Be(cpfFormatado);
        clienteCriado.Endereco.Cidade.Should().Be(command.Cidade);
        repository.Verify(repo => repo.GetByDocumentAsync(cpfFormatado), Times.Once);
        repository.Verify(repo => repo.AddAsync(It.IsAny<Cliente>()), Times.Once);
    }

    [Fact]
    public async Task DeveImpedirCriacaoQuandoCpfJaEstiverCadastrado()
    {
        var command = CriarCommandValido();
        const string cpfFormatado = "529.982.247-25";
        var repository = new Mock<IClienteRepository>();
        repository.Setup(repo => repo.GetByDocumentAsync(cpfFormatado))
            .ReturnsAsync(new Cliente(
                Guid.NewGuid(),
                string.Empty,
                "cliente@teste.local",
                TipoDocumento.Cpf,
                cpfFormatado,
                Guid.NewGuid()));
        var service = new CriarClienteService(repository.Object, new CriarClienteCommandValidator());

        var act = () => service.CriarCliente(command);

        (await act.Should().ThrowAsync<InvalidOperationException>())
            .WithMessage($"Já existe um cliente cadastrado com o CPF {cpfFormatado}.");
        repository.Verify(repo => repo.AddAsync(It.IsAny<Cliente>()), Times.Never);
    }

    [Fact]
    public async Task DeveCriarClienteComCnpjQuandoCommandForValido()
    {
        var command = CriarCommandValido();
        command.TipoDocumento = TipoDocumento.Cnpj;
        command.Documento = "11222333000181";
        const string cnpjFormatado = "11.222.333/0001-81";
        Cliente? clienteCriado = null;
        var repository = new Mock<IClienteRepository>();
        repository.Setup(repo => repo.GetByDocumentAsync(cnpjFormatado))
            .ReturnsAsync((Cliente?)null);
        repository.Setup(repo => repo.AddAsync(It.IsAny<Cliente>()))
            .Callback<Cliente>(cliente => clienteCriado = cliente)
            .Returns<Cliente>(cliente => Task.FromResult(cliente));
        var service = new CriarClienteService(repository.Object, new CriarClienteCommandValidator());

        var clienteId = await service.CriarCliente(command);

        clienteId.Should().NotBeEmpty();
        clienteCriado.Should().NotBeNull();
        clienteCriado.TipoDocumento.Should().Be(TipoDocumento.Cnpj);
        clienteCriado.Documento.Should().Be(cnpjFormatado);
        repository.Verify(repo => repo.GetByDocumentAsync(cnpjFormatado), Times.Once);
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
