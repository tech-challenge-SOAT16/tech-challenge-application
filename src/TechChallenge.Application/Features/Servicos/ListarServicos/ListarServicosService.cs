using TechChallenge.Domain.Entities;
using TechChallenge.Application.Abstractions.Repositories;

namespace TechChallenge.Application.Features.Servicos.ListarServicos;

public class ListarServicosService
{
    private readonly IServicoRepository _servicoRepository;

    public ListarServicosService(IServicoRepository servicoRepository)
    {
        _servicoRepository = servicoRepository;
    }

    public async Task<List<Servico>> ListarServicos(ListarServicosQuery query)
    {
        return await _servicoRepository.GetAllAsync();
    }
}
