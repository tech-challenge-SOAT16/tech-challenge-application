using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using TechChallenge.Api.Models.Request;
using TechChallenge.Api.Models.Response;
using TechChallenge.Application.Abstractions.Notifications;
using TechChallenge.Application.Abstractions.Repositories;
using TechChallenge.Domain.Enums;
using TechChallenge.IntegrationTests.Integration.Factories;
using TechChallenge.Infrastructure.Database.Context;

namespace TechChallenge.IntegrationTests.Integration;

public class OrdensServicoEndpointsIntegrationTests : IClassFixture<WebAplicationFactory<Program>>
{
    private static int _sequencia;
    private readonly WebAplicationFactory<Program> _factory;
    private readonly HttpClient _client;

    public OrdensServicoEndpointsIntegrationTests(WebAplicationFactory<Program> factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task DeveExecutarFluxoBasicoDaMaquinaDeEstados()
    {
        var dados = await CriarDadosBaseAsync();
        var osId = await CriarOrdemServicoAsync(dados);

        await PatchAsync($"/api/v1/ordens-servico/{osId}/atribuir", "Mecanico",
            new AtribuirOrdemServicoRequest { MecanicoId = dados.FuncionarioId });

        await PatchAsync($"/api/v1/ordens-servico/{osId}/diagnostico", "Mecanico",
            new RegistrarDiagnosticoRequest
            {
                Servicos =
                [
                    new ItemDiagnosticoRequest { Id = dados.ServicoId, Quantidade = 1 }
                ]
            });

        await PatchAsync($"/api/v1/ordens-servico/{osId}/orcamento/enviar", "Mecanico");
        await PatchAsync($"/api/v1/ordens-servico/{osId}/aprovar", "Administrador");
        await PatchAsync($"/api/v1/ordens-servico/{osId}/finalizar", "Mecanico");
        await PatchAsync($"/api/v1/ordens-servico/{osId}/entregar", "Administrador");

        var response = await _client.GetAsync($"/api/v1/ordens-servico/{osId}");
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var os = await response.Content.ReadFromJsonAsync<OrdemServicoResponse>(JsonTestOptions.Web);
        os.Should().NotBeNull();
        os.Status.Should().Be(StatusOS.Entregue);
        os.Valor.Should().Be(120);
        os.DataFinalizacao.Should().NotBeNull();
        os.CodigoAcompanhamento.Should().NotBeNullOrWhiteSpace();

        var statusResponse = await _client.GetAsync($"/api/v1/ordens-servico/{osId}/status");
        statusResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var acompanhamentoResponse = await _client.GetAsync($"/api/v1/ordens-servico/acompanhamento/{os.CodigoAcompanhamento}");
        acompanhamentoResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var osAcompanhamento =
            await acompanhamentoResponse.Content.ReadFromJsonAsync<OrdemServicoResponse>(JsonTestOptions.Web);
        osAcompanhamento.Should().NotBeNull();
        osAcompanhamento.Id.Should().Be(osId);

        var metricaResponse = await _client.GetAsync("/api/v1/ordens-servico/tempo-medio-execucao");
        metricaResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var metrica = await metricaResponse.Content.ReadFromJsonAsync<TempoMedioExecucaoResponse>();
        metrica.Should().NotBeNull();
        metrica.QuantidadeOrdensFinalizadas.Should().BeGreaterThan(0);
    }

    [Fact]
    public async Task DeveReceberServicosEProdutosNaAbertura()
    {
        var dados = await CriarDadosBaseAsync();

        var osId = await CriarAsync("/api/v1/ordens-servico", new CriarOrdemServicoRequest
        {
            Descricao = "Revisao preventiva",
            ClienteResponsavelId = dados.ClienteId,
            FuncionarioResponsavelId = dados.FuncionarioId,
            VeiculoId = dados.VeiculoId,
            Servicos = [new ItemDiagnosticoRequest { Id = dados.ServicoId, Quantidade = 1 }],
            Produtos = [new ItemDiagnosticoRequest { Id = dados.ProdutoId, Quantidade = 2 }]
        });

        var response = await _client.GetAsync($"/api/v1/ordens-servico/{osId}");
        var body = await response.Content.ReadAsStringAsync();
        response.StatusCode.Should().Be(HttpStatusCode.OK, body);
        var os = await response.Content.ReadFromJsonAsync<OrdemServicoResponse>(JsonTestOptions.Web);
        os.Should().NotBeNull();
        os.Servicos.Should().ContainSingle(item => item.Id == dados.ServicoId);
        os.Produtos.Should().ContainSingle(item => item.Id == dados.ProdutoId && item.Quantidade == 2);
    }

    [Fact]
    public async Task DeveBloquearFinalizarOSQuandoRoleNaoForMecanico()
    {
        var dados = await CriarDadosBaseAsync();
        var osId = await CriarOrdemServicoAsync(dados);

        var request = new HttpRequestMessage(HttpMethod.Patch, $"/api/v1/ordens-servico/{osId}/finalizar");
        request.Headers.Add(TestAuthHandler.RoleHeader, "Vendedor");

        var response = await _client.SendAsync(request);

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task DeveReceberAprovacaoExternaDeFormaIdempotenteEGerarOutbox()
    {
        var dados = await CriarDadosBaseAsync();
        var osId = await CriarOrdemServicoAsync(dados);

        await PatchAsync($"/api/v1/ordens-servico/{osId}/atribuir", "Mecanico",
            new AtribuirOrdemServicoRequest { MecanicoId = dados.FuncionarioId });
        await PatchAsync($"/api/v1/ordens-servico/{osId}/diagnostico", "Mecanico",
            new RegistrarDiagnosticoRequest
            {
                Servicos = [new ItemDiagnosticoRequest { Id = dados.ServicoId, Quantidade = 1 }]
            });
        await PatchAsync($"/api/v1/ordens-servico/{osId}/orcamento/enviar", "Mecanico");

        var eventoId = $"teste-{Guid.NewGuid():N}";
        var payload = new ReceberDecisaoOrcamentoExternaRequest
        {
            EventoId = eventoId,
            OrdemServicoId = osId,
            Decisao = DecisaoOrcamento.Aprovado,
            OcorridoEm = DateTimeOffset.UtcNow
        };

        var semChave = await _client.PostAsJsonAsync(
            "/api/v1/integracoes/orcamentos/respostas",
            payload);
        semChave.StatusCode.Should().Be(HttpStatusCode.Unauthorized);

        var primeiraResposta = await EnviarDecisaoExternaAsync(payload);
        var corpoPrimeiraResposta = await primeiraResposta.Content.ReadAsStringAsync();
        primeiraResposta.StatusCode.Should().Be(HttpStatusCode.OK, corpoPrimeiraResposta);
        var primeiraDecisao = await primeiraResposta.Content
            .ReadFromJsonAsync<ReceberDecisaoOrcamentoExternaResponse>(JsonTestOptions.Web);
        primeiraDecisao.Should().NotBeNull();
        primeiraDecisao.Processado.Should().BeTrue();
        primeiraDecisao.Duplicado.Should().BeFalse();
        primeiraDecisao.Status.Should().Be(StatusOS.EmExecucao);

        var respostaDuplicada = await EnviarDecisaoExternaAsync(payload);
        respostaDuplicada.StatusCode.Should().Be(HttpStatusCode.OK);
        var decisaoDuplicada = await respostaDuplicada.Content
            .ReadFromJsonAsync<ReceberDecisaoOrcamentoExternaResponse>(JsonTestOptions.Web);
        decisaoDuplicada.Should().NotBeNull();
        decisaoDuplicada.Processado.Should().BeFalse();
        decisaoDuplicada.Duplicado.Should().BeTrue();

        var respostaConflitante = await EnviarDecisaoExternaAsync(new ReceberDecisaoOrcamentoExternaRequest
        {
            EventoId = eventoId,
            OrdemServicoId = osId,
            Decisao = DecisaoOrcamento.Recusado,
            Motivo = "Cliente recusou o orçamento",
            OcorridoEm = payload.OcorridoEm
        });
        respostaConflitante.StatusCode.Should().Be(HttpStatusCode.Conflict);

        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        (await context.DecisaoOrcamentoExterna.CountAsync(item => item.OrdemServicoId == osId))
            .Should().Be(1);
        (await context.NotificacaoStatusOutbox.AnyAsync(item =>
            item.OrdemServicoId == osId &&
            item.StatusAnterior == StatusOS.AguardandoAprovacao &&
            item.StatusAtual == StatusOS.EmExecucao)).Should().BeTrue();

        var outboxRepository = scope.ServiceProvider
            .GetRequiredService<INotificacaoStatusOutboxRepository>();
        var reservadas = await outboxRepository.ReservarPendentesAsync(
            DateTime.UtcNow.AddSeconds(1),
            100,
            TimeSpan.FromSeconds(30));
        var notificacaoDaAprovacao = reservadas.Single(item =>
            item.OrdemServicoId == osId && item.StatusAtual == StatusOS.EmExecucao);
        notificacaoDaAprovacao.BloqueadaAte.Should().NotBeNull();

        notificacaoDaAprovacao.MarcarComoEnviada(DateTime.UtcNow);
        await outboxRepository.SalvarAsync();
        notificacaoDaAprovacao.EnviadaEm.Should().NotBeNull();
    }

    [Fact]
    public async Task DeveConfirmarAprovacaoPeloLinkAssinadoDoEmail()
    {
        var dados = await CriarDadosBaseAsync();
        var osId = await CriarOrdemServicoAsync(dados);

        await PatchAsync($"/api/v1/ordens-servico/{osId}/atribuir", "Mecanico",
            new AtribuirOrdemServicoRequest { MecanicoId = dados.FuncionarioId });
        await PatchAsync($"/api/v1/ordens-servico/{osId}/diagnostico", "Mecanico",
            new RegistrarDiagnosticoRequest
            {
                Servicos = [new ItemDiagnosticoRequest { Id = dados.ServicoId, Quantidade = 1 }]
            });
        await PatchAsync($"/api/v1/ordens-servico/{osId}/orcamento/enviar", "Mecanico");

        string token;
        using (var scope = _factory.Services.CreateScope())
        {
            var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var notificacao = await context.NotificacaoStatusOutbox.SingleAsync(item =>
                item.OrdemServicoId == osId &&
                item.StatusAtual == StatusOS.AguardandoAprovacao);
            var tokenService = scope.ServiceProvider
                .GetRequiredService<IDecisaoOrcamentoTokenService>();
            var criadaEmUtc = DateTime.SpecifyKind(notificacao.CriadaEm, DateTimeKind.Utc);

            token = tokenService.Gerar(
                notificacao.EventoId,
                notificacao.OrdemServicoId,
                new DateTimeOffset(criadaEmUtc),
                TimeSpan.FromHours(48));
        }

        var confirmacao = await _client.GetAsync(
            $"/orcamentos/decisao?token={Uri.EscapeDataString(token)}&decisao=Aprovado");
        var paginaConfirmacao = await confirmacao.Content.ReadAsStringAsync();
        confirmacao.StatusCode.Should().Be(HttpStatusCode.OK, paginaConfirmacao);
        paginaConfirmacao.Should().Contain("Confirmar aprovação");

        var primeiraResposta = await EnviarDecisaoEmailAsync(token, "Aprovado");
        var primeiraPagina = await primeiraResposta.Content.ReadAsStringAsync();
        primeiraResposta.StatusCode.Should().Be(HttpStatusCode.OK, primeiraPagina);
        WebUtility.HtmlDecode(primeiraPagina).Should().Contain("Orçamento aprovado");

        var consulta = await _client.GetAsync($"/api/v1/ordens-servico/{osId}");
        var ordemServico = await consulta.Content
            .ReadFromJsonAsync<OrdemServicoResponse>(JsonTestOptions.Web);
        ordemServico.Should().NotBeNull();
        ordemServico!.Status.Should().Be(StatusOS.EmExecucao);

        var repeticao = await EnviarDecisaoEmailAsync(token, "Aprovado");
        var paginaRepeticao = await repeticao.Content.ReadAsStringAsync();
        repeticao.StatusCode.Should().Be(HttpStatusCode.OK, paginaRepeticao);
        WebUtility.HtmlDecode(paginaRepeticao).Should().Contain("Decisão já registrada");

        var conflito = await EnviarDecisaoEmailAsync(token, "Recusado");
        var paginaConflito = await conflito.Content.ReadAsStringAsync();
        conflito.StatusCode.Should().Be(HttpStatusCode.Conflict, paginaConflito);
        WebUtility.HtmlDecode(paginaConflito).Should().Contain("Decisão não realizada");
    }

    private async Task<DadosBase> CriarDadosBaseAsync()
    {
        var sequencia = Interlocked.Increment(ref _sequencia);

        var clienteId = await CriarAsync("/api/v1/clientes", new CriarClienteRequest
        {
            Nome = "Maria Cliente OS",
            Email = $"maria.os.{sequencia}@teste.local",
            TipoDocumento = TipoDocumento.Cpf,
            Documento = GerarCpf(sequencia * 2),
            Endereco = CriarEnderecoRequest()
        });

        var funcionarioId = await CriarAsync("/api/v1/funcionarios", new CriarFuncionarioRequest
        {
            Nome = "Joao Mecanico OS",
            Cpf = GerarCpf(sequencia * 2 + 1),
            Rg = "987654321",
            Cargo = TipoFuncionario.Mecanico,
            Endereco = CriarEnderecoRequest()
        });

        var categoriaId = await CriarAsync("/api/v1/categoriaveiculo", new CriarCategoriaVeiculoRequest
        {
            Descricao = $"Hatch {sequencia}"
        });

        var veiculoId = await CriarAsync("/api/v1/veiculos", new CriarVeiculoRequest
        {
            Placa = $"ABC{sequencia % 10}D{sequencia % 10}{(sequencia + 1) % 10}",
            Modelo = "Onix",
            Marca = "Chevrolet",
            Cor = "Prata",
            Ano = 2020,
            Quilometragem = 50000,
            Valor = 50000,
            ClienteId = clienteId,
            CategoriaId = categoriaId
        });

        var categoriaServicoId = await CriarAsync("/api/v1/categoriaservico", new CriarCategoriaServicoRequest
        {
            Descricao = $"Mecanica {sequencia}"
        });

        var servicoId = await CriarAsync("/api/v1/servicos", new CriarServicoRequest
        {
            Descricao = "Diagnostico eletronico",
            Valor = 120,
            CategoriaId = categoriaServicoId
        });

        var categoriaProdutoId = await CriarAsync("/api/v1/categoriaproduto", new CriarCategoriaProdutoRequest
        {
            Descricao = $"Filtros {sequencia}"
        });

        var produtoId = await CriarAsync("/api/v1/produtos", new CriarProdutoRequest
        {
            Descricao = $"Filtro de oleo {sequencia}",
            Valor = 45,
            Quantidade = 10,
            IdCategoria = categoriaProdutoId
        });

        return new DadosBase(clienteId, funcionarioId, veiculoId, servicoId, produtoId);
    }

    private async Task<Guid> CriarOrdemServicoAsync(DadosBase dados)
    {
        return await CriarAsync("/api/v1/ordens-servico", new CriarOrdemServicoRequest
        {
            Descricao = "Motor falhando",
            ClienteResponsavelId = dados.ClienteId,
            FuncionarioResponsavelId = dados.FuncionarioId,
            VeiculoId = dados.VeiculoId
        });
    }

    private async Task<Guid> CriarAsync<TRequest>(string rota, TRequest request)
    {
        var response = await _client.PostAsJsonAsync(rota, request);
        var body = await response.Content.ReadAsStringAsync();
        response.StatusCode.Should().Be(HttpStatusCode.Created, body);
        return await response.Content.ReadFromJsonAsync<Guid>();
    }

    private async Task PatchAsync(string rota, string role, object? body = null)
    {
        var request = new HttpRequestMessage(HttpMethod.Patch, rota);
        request.Headers.Add(TestAuthHandler.RoleHeader, role);

        if (body is not null)
            request.Content = JsonContent.Create(body);

        var response = await _client.SendAsync(request);
        var responseBody = await response.Content.ReadAsStringAsync();
        response.StatusCode.Should().Be(HttpStatusCode.OK, responseBody);
    }

    private async Task<HttpResponseMessage> EnviarDecisaoExternaAsync(
        ReceberDecisaoOrcamentoExternaRequest payload)
    {
        var request = new HttpRequestMessage(
            HttpMethod.Post,
            "/api/v1/integracoes/orcamentos/respostas")
        {
            Content = JsonContent.Create(payload)
        };
        request.Headers.Add("X-Integration-Key", "integration-test-key");
        return await _client.SendAsync(request);
    }

    private Task<HttpResponseMessage> EnviarDecisaoEmailAsync(string token, string decisao)
    {
        return _client.PostAsync(
            "/orcamentos/decisao",
            new FormUrlEncodedContent(new Dictionary<string, string>
            {
                ["token"] = token,
                ["decisao"] = decisao
            }));
    }

    private static EnderecoRequest CriarEnderecoRequest()
    {
        return new EnderecoRequest
        {
            Logradouro = "Rua Teste",
            Complemento = "Casa",
            Numero = "100",
            Bairro = "Centro",
            Cidade = "Sao Paulo",
            Estado = "SP",
            Cep = "01001000"
        };
    }

    private static string GerarCpf(int seed)
    {
        var digitos = new int[11];
        var valor = Math.Abs(seed) + 12345678;

        for (var i = 7; i >= 0; i--)
        {
            digitos[i] = valor % 10;
            valor /= 10;
        }

        digitos[8] = seed % 10;
        digitos[9] = CalcularDigito(digitos, 9);
        digitos[10] = CalcularDigito(digitos, 10);

        return string.Concat(digitos);
    }

    private static int CalcularDigito(int[] digitos, int tamanho)
    {
        var soma = 0;
        for (var i = 0; i < tamanho; i++)
            soma += digitos[i] * (tamanho + 1 - i);

        var resto = soma % 11;
        return resto < 2 ? 0 : 11 - resto;
    }

    private sealed record DadosBase(
        Guid ClienteId,
        Guid FuncionarioId,
        Guid VeiculoId,
        Guid ServicoId,
        Guid ProdutoId);

    private sealed class TempoMedioExecucaoResponse
    {
        public int QuantidadeOrdensFinalizadas { get; set; }
        public double TempoMedioMinutos { get; set; }
        public double TempoMedioHoras { get; set; }
    }
}
