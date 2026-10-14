using FluentValidation;
using TechChallenge.Application.Abstractions.Notifications;
using TechChallenge.Application.Abstractions.Repositories;
using TechChallenge.Application.Notifications;
using TechChallenge.Domain.Entities;
using TechChallenge.Domain.Enums;

namespace TechChallenge.Application.Features.OS.FinalizarOS;

public class FinalizarOSService
{
    private readonly IOrdemServicoRepository _ordemServicoRepository;
    private readonly IEstoqueRepository _estoqueRepository;
    private readonly IValidator<FinalizarOSCommand> _validator;
    private readonly INotificationService _notificationService;

    public FinalizarOSService(
        IOrdemServicoRepository ordemServicoRepository,
        IEstoqueRepository estoqueRepository,
        IValidator<FinalizarOSCommand> validator,
        INotificationService? notificationService = null)
    {
        _ordemServicoRepository = ordemServicoRepository;
        _estoqueRepository = estoqueRepository;
        _validator = validator;
        _notificationService = notificationService ?? NullNotificationService.Instance;
    }

    public async Task<bool> FinalizarOS(FinalizarOSCommand command)
    {
        List<Domain.Entities.Estoque> estoqueEntity = new List<Domain.Entities.Estoque>();

        _validator.ValidateAndThrow(command);

        var os = await _ordemServicoRepository.GetByIdAsync(command.OrdemServicoId);
        if (os is null)
            throw new KeyNotFoundException($"OS com Id {command.OrdemServicoId} não encontrada.");

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

            estoqueEntity.Add(estoque);
        }            

        foreach (var item in os.Produtos)
        {
            var estoque = estoqueEntity.FirstOrDefault(e => e.ProdutoId == item.ProdutoId);
            
            estoque!.Baixar(item.Quantidade);
            await _estoqueRepository.UpdateAsync(estoque);
        }

        var statusAnterior = os.Status;

        os.TransicionarPara(StatusOS.Finalizada);
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
