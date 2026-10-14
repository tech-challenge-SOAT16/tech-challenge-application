using FluentAssertions;
using TechChallenge.Domain.Entities;
using TechChallenge.Domain.Enums;
using TechChallenge.Domain.Exceptions;

namespace TechChallenge.Tests.Domain;

public class OrdemServicoDecisaoExternaTests
{
    [Fact]
    public void DeveAprovarOrcamentoExternoERegistrarEventoDeStatus()
    {
        var ordemServico = CriarAguardandoAprovacao();
        var ocorridoEm = DateTime.UtcNow.AddSeconds(-1);

        var processado = ordemServico.ReceberDecisaoExterna(
            "evento-1",
            DecisaoOrcamento.Aprovado,
            null,
            ocorridoEm,
            DateTime.UtcNow);

        processado.Should().BeTrue();
        ordemServico.Status.Should().Be(StatusOS.EmExecucao);
        ordemServico.DecisoesExternas.Should().ContainSingle(item =>
            item.EventoId == "evento-1" && item.Decisao == DecisaoOrcamento.Aprovado);
        ordemServico.EventosDominio.Should().ContainSingle(item =>
            item.StatusAnterior == StatusOS.AguardandoAprovacao &&
            item.StatusAtual == StatusOS.EmExecucao);
    }

    [Fact]
    public void DeveIgnorarRepeticaoIdenticaERejeitarEventoComConteudoDiferente()
    {
        var ordemServico = CriarAguardandoAprovacao();
        var ocorridoEm = DateTime.UtcNow.AddSeconds(-1);

        ordemServico.ReceberDecisaoExterna(
            "evento-2",
            DecisaoOrcamento.Recusado,
            "  Valor não aprovado  ",
            ocorridoEm,
            DateTime.UtcNow);
        ordemServico.LimparEventosDominio();

        var duplicado = ordemServico.ReceberDecisaoExterna(
            "evento-2",
            DecisaoOrcamento.Recusado,
            "Valor não aprovado",
            ocorridoEm,
            DateTime.UtcNow);

        duplicado.Should().BeFalse();
        ordemServico.Status.Should().Be(StatusOS.Reprovada);
        ordemServico.DecisoesExternas.Should().HaveCount(1);
        ordemServico.EventosDominio.Should().BeEmpty();

        var acaoConflitante = () => ordemServico.ReceberDecisaoExterna(
            "evento-2",
            DecisaoOrcamento.Aprovado,
            null,
            ocorridoEm,
            DateTime.UtcNow);

        acaoConflitante.Should().Throw<DomainConflictException>();
    }

    private static OrdemServico CriarAguardandoAprovacao()
    {
        var ordemServico = new OrdemServico(
            Guid.NewGuid(),
            "Diagnóstico",
            "OS-TESTE",
            StatusOS.Recebida,
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            DateTime.UtcNow,
            null,
            null,
            100,
            0,
            0);

        ordemServico.TransicionarPara(StatusOS.EmDiagnostico);
        ordemServico.TransicionarPara(StatusOS.AguardandoAprovacao);
        ordemServico.LimparEventosDominio();
        return ordemServico;
    }
}
