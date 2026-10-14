using TechChallenge.Application.Abstractions.Repositories;
using TechChallenge.Domain.Entities;

namespace TechChallenge.Application.Features.OS.ObterOS;

public class ObterOSService
{
    private readonly IOrdemServicoRepository _ordemServicoRepository;

    public ObterOSService(IOrdemServicoRepository ordemServicoRepository)
    {
        _ordemServicoRepository = ordemServicoRepository;
    }

    public async Task<OrdemServico> ObterOS(ObterOSQuery query)
    {
        var ordemServico = await _ordemServicoRepository.GetByIdAsync(query.Id);
        if (ordemServico is null)
            throw new KeyNotFoundException($"Ordem de serviço com Id {query.Id} não encontrada.");

        return ordemServico;
    }
}
