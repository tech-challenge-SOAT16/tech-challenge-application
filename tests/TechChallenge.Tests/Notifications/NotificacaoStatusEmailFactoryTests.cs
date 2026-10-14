using System.Net;
using FluentAssertions;
using Microsoft.Extensions.Options;
using Moq;
using TechChallenge.Application.Abstractions.Notifications;
using TechChallenge.Domain.Entities;
using TechChallenge.Domain.Enums;
using TechChallenge.Domain.Events;
using TechChallenge.Infrastructure.Notifications;

namespace TechChallenge.Tests.Notifications;

public class NotificacaoStatusEmailFactoryTests
{
    [Fact]
    public void DeveAdicionarBotoesQuandoOrcamentoAguardarAprovacao()
    {
        var tokenService = new Mock<IDecisaoOrcamentoTokenService>();
        tokenService
            .Setup(service => service.Gerar(
                It.IsAny<Guid>(),
                It.IsAny<Guid>(),
                It.IsAny<DateTimeOffset>(),
                It.IsAny<TimeSpan>()))
            .Returns("token-assinado");
        var factory = CriarFactory(tokenService.Object);
        var notificacao = CriarNotificacao(StatusOS.AguardandoAprovacao);
        var orcamento = new OrcamentoEmailResumo(
            [new OrcamentoEmailItem("Diagnóstico eletrônico", 1, 120)],
            [new OrcamentoEmailItem("Filtro de óleo", 2, 90)],
            0,
            10,
            200);

        var email = factory.Criar(notificacao, "Cliente Teste", orcamento);
        var conteudoDecodificado = WebUtility.HtmlDecode(email.ConteudoHtml);

        email.Assunto.Should().Be("Ação necessária: aprove ou recuse o orçamento");
        email.Assunto.Should().NotContain(notificacao.CodigoAcompanhamento);
        email.ConteudoHtml.Should().Contain("Aprovar orçamento");
        email.ConteudoHtml.Should().Contain("Recusar orçamento");
        conteudoDecodificado.Should().Contain("Resumo do orçamento");
        conteudoDecodificado.Should().Contain("Diagnóstico eletrônico");
        conteudoDecodificado.Should().Contain("Filtro de óleo (2x)");
        conteudoDecodificado.Should().Contain("R$ 200,00");
        email.ConteudoHtml.Should().Contain("token=token-assinado&amp;decisao=Aprovado");
        email.ConteudoHtml.Should().Contain("válidos por 48 horas");
    }

    [Fact]
    public void NaoDeveAdicionarBotoesNasDemaisMudancasDeStatus()
    {
        var tokenService = new Mock<IDecisaoOrcamentoTokenService>();
        var factory = CriarFactory(tokenService.Object);
        var notificacao = CriarNotificacao(StatusOS.EmExecucao);

        var email = factory.Criar(notificacao, "Cliente Teste");

        email.Assunto.Should().Be("OS Atualizada: Em Execução");
        email.Assunto.Should().NotContain(notificacao.CodigoAcompanhamento);
        email.ConteudoHtml.Should().NotContain("Aprovar orçamento");
        tokenService.VerifyNoOtherCalls();
    }

    private static NotificacaoStatusEmailFactory CriarFactory(
        IDecisaoOrcamentoTokenService tokenService) =>
        new(
            tokenService,
            Options.Create(new ApprovalLinkOptions
            {
                BaseUrl = "http://localhost:8080",
                ExpirationHours = 48
            }));

    private static NotificacaoStatusOutbox CriarNotificacao(StatusOS statusAtual) =>
        new(
            Guid.NewGuid(),
            new StatusOrdemServicoAlteradoEvent(
                Guid.NewGuid(),
                Guid.NewGuid(),
                Guid.NewGuid(),
                "OS-TESTE",
                StatusOS.EmDiagnostico,
                statusAtual,
                DateTime.UtcNow));
}
