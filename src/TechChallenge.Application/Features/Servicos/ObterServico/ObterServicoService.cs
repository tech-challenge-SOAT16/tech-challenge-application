using TechChallenge.Domain.Entities;
using TechChallenge.Application.Abstractions.Repositories;

namespace TechChallenge.Application.Features.Servicos.ObterServico;

public class ObterServicoService
{
    private readonly IServicoRepository _servicoRepository;

    public ObterServicoService(IServicoRepository servicoRepository)
    {
        _servicoRepository = servicoRepository;
    }

    public async Task<Servico> ObterServico(ObterServicoQuery query)
    {
        var servico = await _servicoRepository.GetByIdAsync(query.Id);
        if (servico is null)
            throw new KeyNotFoundException($"Serviço com Id {query.Id} não encontrado.");

        return servico;
    }
}
