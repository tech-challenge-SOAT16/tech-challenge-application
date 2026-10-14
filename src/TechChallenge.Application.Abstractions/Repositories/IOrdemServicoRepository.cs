using System;
using TechChallenge.Domain.Entities;
using TechChallenge.Domain.Enums;

namespace TechChallenge.Application.Abstractions.Repositories
{
    public interface IOrdemServicoRepository : IRepository<OrdemServico>
    {
        Task<List<OrdemServico>> GetByStatusAsync(StatusOS status);
        Task<OrdemServico?> GetOSAtivaMecanicoAsync(Guid mecanicoId);
        Task<bool> ExistePorClienteAsync(Guid clienteId);
        Task<bool> ExistePorFuncionarioAsync(Guid funcionarioId);
        Task<bool> ExistePorVeiculoAsync(Guid veiculoId);
        Task<OrdemServico?> GetByCodigoAcompanhamentoAsync(string codigoAcompanhamento);
        Task<DecisaoOrcamentoExterna?> GetDecisaoExternaPorEventoIdAsync(string eventoId);
        Task<List<OrdemServico>> GetFinalizadasComDataFinalizacaoAsync();
    }
}
