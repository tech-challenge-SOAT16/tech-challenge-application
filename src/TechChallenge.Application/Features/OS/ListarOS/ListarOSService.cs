using TechChallenge.Domain.Entities;
using TechChallenge.Application.Abstractions.Repositories;

namespace TechChallenge.Application.Features.OS.ListarOS;

public class ListarOSService
{
    private readonly IOrdemServicoRepository _ordemServicoRepository;

    public ListarOSService(IOrdemServicoRepository ordemServicoRepository)
    {
        _ordemServicoRepository = ordemServicoRepository;
    }

    public async Task<List<OrdemServico>> ListarOS(ListarOSQuery query)
    {
        if (query.Status.HasValue)
            return await _ordemServicoRepository.GetByStatusAsync(query.Status.Value);

        return await _ordemServicoRepository.GetAllAsync();
    }
}