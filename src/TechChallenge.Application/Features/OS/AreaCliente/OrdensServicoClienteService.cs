using TechChallenge.Application.Abstractions.Repositories;
using TechChallenge.Domain.Entities;

namespace TechChallenge.Application.Features.OS.AreaCliente;

/// <summary>
/// Consultas do cliente autenticado por CPF: ele só enxerga as próprias OS.
/// </summary>
public class OrdensServicoClienteService
{
    private readonly IOrdemServicoRepository _ordemServicoRepository;

    public OrdensServicoClienteService(IOrdemServicoRepository ordemServicoRepository)
    {
        _ordemServicoRepository = ordemServicoRepository;
    }

    public Task<List<OrdemServico>> ListarAsync(Guid clienteId) =>
        _ordemServicoRepository.GetByClienteAsync(clienteId);

    public async Task<OrdemServico> ObterAsync(Guid clienteId, Guid ordemServicoId)
    {
        var ordemServico = await _ordemServicoRepository.GetByIdAsync(ordemServicoId);

        // OS de outro cliente responde como inexistente para não revelar que ela existe.
        if (ordemServico is null || ordemServico.ClienteResponsavelId != clienteId)
            throw new KeyNotFoundException($"OS com Id {ordemServicoId} não encontrada.");

        return ordemServico;
    }
}
