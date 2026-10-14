using System;
using FluentValidation;
using TechChallenge.Application.Abstractions.Notifications;
using TechChallenge.Application.Abstractions.Repositories;
using TechChallenge.Application.Notifications;
using TechChallenge.Domain.Entities;
using TechChallenge.Domain.Enums;

namespace TechChallenge.Application.Features.OS.RegistrarDiagnostico;

public class RegistrarDiagnosticoService
{
    private readonly IOrdemServicoRepository _ordemServicoRepository;
    private readonly IServicoRepository _servicoRepository;
    private readonly IProdutoRepository _produtoRepository;
    private readonly IEstoqueRepository _estoqueRepository;
    private readonly IValidator<RegistrarDiagnosticoCommand> _validator;
    private readonly INotificationService _notificationService;

    public RegistrarDiagnosticoService(
        IOrdemServicoRepository ordemServicoRepository,
        IServicoRepository servicoRepository,
        IProdutoRepository produtoRepository,
        IEstoqueRepository estoqueRepository,
        IValidator<RegistrarDiagnosticoCommand> validator,
        INotificationService? notificationService = null)
    {
        _ordemServicoRepository = ordemServicoRepository;
        _servicoRepository = servicoRepository;
        _produtoRepository = produtoRepository;
        _estoqueRepository = estoqueRepository;
        _validator = validator;
        _notificationService = notificationService ?? NullNotificationService.Instance;
    }

    // RF11: registra serviços e produtos na OS
    public async Task<bool> RegistrarDiagnostico(RegistrarDiagnosticoCommand command)
    {
        _validator.ValidateAndThrow(command);

        var os = await _ordemServicoRepository.GetByIdAsync(command.OrdemServicoId);
        if (os is null)
            throw new KeyNotFoundException($"OS com Id {command.OrdemServicoId} não encontrada.");

        if (os.Status != StatusOS.EmDiagnostico)
            throw new InvalidOperationException($"Apenas OS com status Em Diagnóstico aceitam registro. Status atual: {os.Status}.");

        foreach (var item in command.Servicos)
        {
            var servico = await _servicoRepository.GetByIdAsync(item.Id);
            if (servico is null)
                throw new KeyNotFoundException($"Serviço com Id {item.Id} não encontrado.");

            os.AdicionarServicos(new OrdemServicoServicos(
                Guid.Empty, os.Id, servico.Id, (double)servico.Valor, item.Quantidade, 0, 0));
        }

        foreach (var item in command.Produtos)
        {
            var produto = await _produtoRepository.GetByIdAsync(item.Id);
            if (produto is null)
                throw new KeyNotFoundException($"Produto com Id {item.Id} não encontrado.");

            var estoque = await _estoqueRepository.GetByIdProdutoAsync(item.Id);

            if (estoque is null)
            {
                NotificarEstoqueInsuficiente(os, produto.Descricao, 0, item.Quantidade);
                throw new InvalidOperationException($"Estoque insuficiente para o produto {produto.Descricao}.");
            }
            else if (estoque.Quantidade < item.Quantidade)
            {
                NotificarEstoqueInsuficiente(os, produto.Descricao, estoque.Quantidade, item.Quantidade);
                throw new InvalidOperationException($"Estoque insuficiente para o produto {produto.Descricao}.");
            }

            os.AdicionarProdutos(new OrdemServicoProdutos(
                Guid.Empty, os.Id, produto.Id, (double)produto.Valor, item.Quantidade, 0, 0));
        }

        os.AtualizarData(DateTime.UtcNow);

        await _ordemServicoRepository.UpdateAsync(os);
        return true;
    }

    private void NotificarEstoqueInsuficiente(OrdemServico os, string produtoDescricao, double quantidadeDisponivel, double quantidadeNecessaria)
    {
        var mensagem = $"OS {os.Id} precisa de {quantidadeNecessaria} unidade(s) de {produtoDescricao}, mas há {quantidadeDisponivel} em estoque.";

        _notificationService.NotificarFuncionariosPorFuncao(
            TipoFuncionario.Administrador,
            "Estoque insuficiente",
            mensagem);

        _notificationService.NotificarFuncionario(
            os.FuncionarioResponsavelId,
            "Estoque insuficiente",
            mensagem);
    }
}
