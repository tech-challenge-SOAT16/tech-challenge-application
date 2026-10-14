using FluentValidation;
using TechChallenge.Application.Abstractions.Repositories;
using TechChallenge.Domain.Enums;
using TechChallenge.Domain.Exceptions;

namespace TechChallenge.Application.Features.OS.ReceberDecisaoOrcamentoExterna;

public class ReceberDecisaoOrcamentoExternaService
{
    private readonly IOrdemServicoRepository _ordemServicoRepository;
    private readonly IValidator<ReceberDecisaoOrcamentoExternaCommand> _validator;

    public ReceberDecisaoOrcamentoExternaService(
        IOrdemServicoRepository ordemServicoRepository,
        IValidator<ReceberDecisaoOrcamentoExternaCommand> validator)
    {
        _ordemServicoRepository = ordemServicoRepository;
        _validator = validator;
    }

    public async Task<ReceberDecisaoOrcamentoExternaResult> ReceberAsync(
        ReceberDecisaoOrcamentoExternaCommand command)
    {
        await _validator.ValidateAndThrowAsync(command);

        var eventoId = command.EventoId.Trim();
        var decisaoExistente = await _ordemServicoRepository
            .GetDecisaoExternaPorEventoIdAsync(eventoId);

        if (decisaoExistente is not null)
        {
            if (decisaoExistente.OrdemServicoId != command.OrdemServicoId ||
                !decisaoExistente.CorrespondeA(
                    command.Decisao,
                    command.Motivo,
                    command.OcorridoEm.UtcDateTime))
            {
                throw new DomainConflictException(
                    $"O evento externo {eventoId} já foi registrado com outro conteúdo.");
            }

            return new ReceberDecisaoOrcamentoExternaResult(
                eventoId,
                decisaoExistente.OrdemServicoId,
                command.Decisao == DecisaoOrcamento.Aprovado
                    ? StatusOS.EmExecucao
                    : StatusOS.Reprovada,
                false,
                true);
        }

        var ordemServico = await _ordemServicoRepository.GetByIdAsync(command.OrdemServicoId);
        if (ordemServico is null)
            throw new KeyNotFoundException($"OS com Id {command.OrdemServicoId} não encontrada.");

        var processado = ordemServico.ReceberDecisaoExterna(
            eventoId,
            command.Decisao,
            command.Motivo,
            command.OcorridoEm.UtcDateTime,
            DateTime.UtcNow);

        if (processado)
            await _ordemServicoRepository.UpdateAsync(ordemServico);

        return new ReceberDecisaoOrcamentoExternaResult(
            eventoId,
            ordemServico.Id,
            ordemServico.Status,
            processado,
            !processado);
    }
}
