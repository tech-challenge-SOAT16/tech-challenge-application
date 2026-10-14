using TechChallenge.Application.Abstractions.Repositories;
using TechChallenge.Domain.Entities;

namespace TechChallenge.Application.Features.OS.ObterOSAcompanhamento;

public class ObterOSAcompanhamentoService
{
    private readonly IOrdemServicoRepository _ordemServicoRepository;

    public ObterOSAcompanhamentoService(IOrdemServicoRepository ordemServicoRepository)
    {
        _ordemServicoRepository = ordemServicoRepository;
    }

    public async Task<OrdemServico> ObterOSAcompanhamento(ObterOSAcompanhamentoQuery query)
    {
        var ordemServico = await _ordemServicoRepository
            .GetByCodigoAcompanhamentoAsync(query.CodigoAcompanhamento);

        if (ordemServico is null)
            throw new KeyNotFoundException("Ordem de serviço não encontrada para o código de acompanhamento informado.");

        return ordemServico;
    }
}
