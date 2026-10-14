using TechChallenge.Application.Abstractions.Repositories;
using TechChallenge.Domain.Enums;

namespace TechChallenge.Application.Features.OS.ListarOSOficina;

public class ListarOSOficinaService
{
    private readonly IOrdemServicoRepository _ordemServicoRepository;

    public ListarOSOficinaService(IOrdemServicoRepository ordemServicoRepository)
    {
        _ordemServicoRepository = ordemServicoRepository;
    }

    public async Task<List<ListarOSOficinaResponseDto>> ListarOSOficina()
    {
        var ordens = await _ordemServicoRepository.GetByStatusAsync(StatusOS.EmDiagnostico);

        return ordens.Select(os => new ListarOSOficinaResponseDto
        {
            Id = os.Id,
            PlacaVeiculo = os.Veiculo.Placa,
            NomeMecanico = os.FuncionarioResponsavel.Nome,
            RelatoInicial = os.Descricao
        }).ToList();
    }
}
