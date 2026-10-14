using FluentValidation;
using TechChallenge.Application.Abstractions.Notifications;
using TechChallenge.Application.Abstractions.Repositories;
using TechChallenge.Application.Notifications;
using TechChallenge.Domain.Enums;

namespace TechChallenge.Application.Features.OS.EnviarOrcamento;

public class EnviarOrcamentoService
{
    private readonly IOrdemServicoRepository _ordemServicoRepository;
    private readonly IEstoqueRepository _estoqueRepository;
    private readonly IValidator<EnviarOrcamentoCommand> _validator;
    private readonly INotificationService _notificationService;

    public EnviarOrcamentoService(
        IOrdemServicoRepository ordemServicoRepository,
        IEstoqueRepository estoqueRepository,
        IValidator<EnviarOrcamentoCommand> validator,
        INotificationService? notificationService = null)
    {
        _ordemServicoRepository = ordemServicoRepository;
        _estoqueRepository = estoqueRepository;
        _validator = validator;
        _notificationService = notificationService ?? NullNotificationService.Instance;
    }

    public async Task<bool> EnviarOrcamento(EnviarOrcamentoCommand command)
    {
        _validator.ValidateAndThrow(command);

        var os = await _ordemServicoRepository.GetByIdAsync(command.OrdemServicoId);
        if (os is null)
            throw new KeyNotFoundException($"OS com Id {command.OrdemServicoId} não encontrada.");

        if (os.Servicos.Count == 0 && os.Produtos.Count == 0)
            throw new InvalidOperationException("Informe ao menos um serviço ou produto antes de enviar o orçamento.");

        foreach (var item in os.Produtos)
        {
            var estoque = await _estoqueRepository.GetByIdProdutoAsync(item.ProdutoId);
            var produtoDescricao = item.Produto?.Descricao ?? item.ProdutoId.ToString();

            if (estoque is null || estoque.Quantidade < item.Quantidade)
            {
                NotificarEstoqueInsuficiente(
                    os.Id,
                    os.FuncionarioResponsavelId,
                    produtoDescricao,
                    estoque?.Quantidade ?? 0,
                    item.Quantidade);

                throw new InvalidOperationException($"Estoque insuficiente para o produto {produtoDescricao}.");
            }
        }

        os.AtribuirValor(
            os.Servicos.Sum(item => (item.Valor * item.Quantidade) + item.Acrescimo - item.Desconto) +
            os.Produtos.Sum(item => (item.Valor * item.Quantidade) + item.Acrescimo - item.Desconto) +
            os.Acrescimo -
            os.Desconto);

        var statusAnterior = os.Status;

        os.TransicionarPara(StatusOS.AguardandoAprovacao);
        _notificationService.NotificarTransicaoOS(os, statusAnterior);
        await _ordemServicoRepository.UpdateAsync(os);
        return true;
    }

    private void NotificarEstoqueInsuficiente(Guid ordemServicoId, Guid funcionarioResponsavelId, string produtoDescricao, double quantidadeDisponivel, double quantidadeNecessaria)
    {
        var mensagem = $"OS {ordemServicoId} precisa de {quantidadeNecessaria} unidade(s) de {produtoDescricao}, mas há {quantidadeDisponivel} em estoque.";

        _notificationService.NotificarFuncionariosPorFuncao(
            TipoFuncionario.Administrador,
            "Estoque insuficiente",
            mensagem);

        _notificationService.NotificarFuncionario(
            funcionarioResponsavelId,
            "Estoque insuficiente",
            mensagem);
    }
}
