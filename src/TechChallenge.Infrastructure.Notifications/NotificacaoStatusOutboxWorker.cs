using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using TechChallenge.Application.Abstractions.Notifications;
using TechChallenge.Application.Abstractions.Repositories;
using TechChallenge.Domain.Entities;
using TechChallenge.Domain.Enums;

namespace TechChallenge.Infrastructure.Notifications;

public class NotificacaoStatusOutboxWorker : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly NotificationWorkerOptions _options;
    private readonly NotificacaoStatusEmailFactory _emailFactory;
    private readonly ILogger<NotificacaoStatusOutboxWorker> _logger;

    public NotificacaoStatusOutboxWorker(
        IServiceScopeFactory scopeFactory,
        IOptions<NotificationWorkerOptions> options,
        NotificacaoStatusEmailFactory emailFactory,
        ILogger<NotificacaoStatusOutboxWorker> logger)
    {
        _scopeFactory = scopeFactory;
        _options = options.Value;
        _emailFactory = emailFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await ProcessarLoteAsync(stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception exception)
            {
                _logger.LogError(exception, "Falha inesperada ao processar notificações da outbox.");
            }

            await Task.Delay(TimeSpan.FromSeconds(_options.IntervalSeconds), stoppingToken);
        }
    }

    private async Task ProcessarLoteAsync(CancellationToken cancellationToken)
    {
        using var scope = _scopeFactory.CreateScope();
        var repository = scope.ServiceProvider.GetRequiredService<INotificacaoStatusOutboxRepository>();
        var ordemServicoRepository = scope.ServiceProvider.GetRequiredService<IOrdemServicoRepository>();
        var emailSender = scope.ServiceProvider.GetRequiredService<IEmailSender>();
        var agora = DateTime.UtcNow;

        var notificacoes = await repository.ReservarPendentesAsync(
            agora,
            _options.BatchSize,
            TimeSpan.FromSeconds(_options.LockSeconds),
            cancellationToken);

        foreach (var notificacao in notificacoes)
        {
            await ProcessarNotificacaoAsync(
                notificacao,
                repository,
                ordemServicoRepository,
                emailSender,
                cancellationToken);
        }
    }

    private async Task ProcessarNotificacaoAsync(
        NotificacaoStatusOutbox notificacao,
        INotificacaoStatusOutboxRepository repository,
        IOrdemServicoRepository ordemServicoRepository,
        IEmailSender emailSender,
        CancellationToken cancellationToken)
    {
        try
        {
            var orcamento = await ObterResumoOrcamentoAsync(
                notificacao,
                ordemServicoRepository);
            var email = _emailFactory.Criar(
                notificacao,
                notificacao.Cliente.Nome,
                orcamento);

            await emailSender.EnviarAsync(
                notificacao.Cliente.Email,
                email.Assunto,
                email.ConteudoHtml,
                isHtml: true,
                cancellationToken: cancellationToken);

            notificacao.MarcarComoEnviada(DateTime.UtcNow);
            await repository.SalvarAsync(cancellationToken);

            _logger.LogInformation(
                "Notificação {NotificacaoId} da OS {OrdemServicoId} enviada com sucesso.",
                notificacao.Id,
                notificacao.OrdemServicoId);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception exception)
        {
            notificacao.RegistrarFalha(exception.Message, DateTime.UtcNow);
            await repository.SalvarAsync(cancellationToken);

            _logger.LogWarning(
                exception,
                "Falha na tentativa {Tentativa} de enviar a notificação {NotificacaoId}.",
                notificacao.Tentativas,
                notificacao.Id);
        }
    }

    private static async Task<OrcamentoEmailResumo?> ObterResumoOrcamentoAsync(
        NotificacaoStatusOutbox notificacao,
        IOrdemServicoRepository repository)
    {
        if (notificacao.StatusAtual != StatusOS.AguardandoAprovacao)
            return null;

        var ordemServico = await repository.GetByIdAsync(notificacao.OrdemServicoId)
            ?? throw new KeyNotFoundException(
                $"OS {notificacao.OrdemServicoId} não encontrada para montar o orçamento.");

        var servicos = ordemServico.Servicos
            .Select(item => new OrcamentoEmailItem(
                item.Servico.Descricao,
                item.Quantidade,
                (item.Valor * item.Quantidade) + item.Acrescimo - item.Desconto))
            .ToList();
        var produtos = ordemServico.Produtos
            .Select(item => new OrcamentoEmailItem(
                item.Produto.Descricao,
                item.Quantidade,
                (item.Valor * item.Quantidade) + item.Acrescimo - item.Desconto))
            .ToList();

        return new OrcamentoEmailResumo(
            servicos,
            produtos,
            ordemServico.Acrescimo,
            ordemServico.Desconto,
            ordemServico.Valor);
    }
}
