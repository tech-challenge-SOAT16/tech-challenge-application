using TechChallenge.Application.Abstractions.Repositories;

namespace TechChallenge.Application.Features.OS.ObterTempoMedioExecucao;

public class ObterTempoMedioExecucaoService
{
    private readonly IOrdemServicoRepository _ordemServicoRepository;

    public ObterTempoMedioExecucaoService(IOrdemServicoRepository ordemServicoRepository)
    {
        _ordemServicoRepository = ordemServicoRepository;
    }

    public async Task<TempoMedioExecucaoResponseDto> ObterTempoMedioExecucao()
    {
        var ordens = await _ordemServicoRepository.GetFinalizadasComDataFinalizacaoAsync();
        var duracoes = ordens
            .Where(os => os.DataFinalizacao.HasValue)
            .Select(os => os.DataFinalizacao!.Value - os.DataCriacao)
            .ToList();

        if (duracoes.Count == 0)
            return new TempoMedioExecucaoResponseDto();

        var tempoMedioMinutos = duracoes.Average(duracao => duracao.TotalMinutes);

        return new TempoMedioExecucaoResponseDto
        {
            QuantidadeOrdensFinalizadas = duracoes.Count,
            TempoMedioMinutos = Math.Round(tempoMedioMinutos, 2),
            TempoMedioHoras = Math.Round(tempoMedioMinutos / 60, 2)
        };
    }
}
