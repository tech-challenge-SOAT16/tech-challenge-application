using TechChallenge.Domain.Enums;
using TechChallenge.Domain.Events;
using TechChallenge.Domain.Exceptions;

namespace TechChallenge.Domain.Entities;

public class OrdemServico
{
    private readonly List<StatusOrdemServicoAlteradoEvent> _eventosDominio = [];

    private static readonly Dictionary<StatusOS, StatusOS[]> Transicoes = new()
    {
        [StatusOS.Recebida] = [StatusOS.EmDiagnostico, StatusOS.Cancelada],
        [StatusOS.EmDiagnostico] = [StatusOS.AguardandoAprovacao, StatusOS.Cancelada],
        [StatusOS.AguardandoAprovacao] = [StatusOS.EmExecucao, StatusOS.Reprovada, StatusOS.Cancelada],
        [StatusOS.Reprovada] = [StatusOS.EmDiagnostico, StatusOS.Cancelada],
        [StatusOS.EmExecucao] = [StatusOS.Finalizada, StatusOS.Cancelada],
        [StatusOS.Finalizada] = [StatusOS.Entregue],
        [StatusOS.Entregue] = [],
        [StatusOS.Cancelada] = []
    };

    public Guid Id { get; private set; }
    public string Descricao { get; private set; } = string.Empty;
    public string CodigoAcompanhamento { get; private set; } = string.Empty;
    public StatusOS Status { get; private set; }
    public Guid ClienteResponsavelId { get; private set; }
    public Cliente ClienteResponsavel { get; private set; } = null!;
    public Guid FuncionarioResponsavelId { get; private set; }
    public Funcionario FuncionarioResponsavel { get; private set; } = null!;
    public Guid VeiculoId { get; private set; }
    public Veiculo Veiculo { get; private set; } = null!;
    public ICollection<OrdemServicoServicos> Servicos { get; private set; } = new List<OrdemServicoServicos>();
    public ICollection<OrdemServicoProdutos> Produtos { get; private set; } = new List<OrdemServicoProdutos>();
    public ICollection<DecisaoOrcamentoExterna> DecisoesExternas { get; private set; } = new List<DecisaoOrcamentoExterna>();
    public IReadOnlyCollection<StatusOrdemServicoAlteradoEvent> EventosDominio => _eventosDominio.AsReadOnly();
    public DateTime DataCriacao { get; private set; }
    public DateTime? DataAtualizacao { get; private set; }
    public DateTime? DataFinalizacao { get; private set; }
    public double Valor { get; private set; }
    public double Desconto { get; private set; }
    public double Acrescimo { get; private set; }

    public OrdemServico(Guid id, string descricao, string codigoAcompanhamento, StatusOS status, Guid clienteResponsavelId, Guid funcionarioResponsavelId, Guid veiculoId, DateTime dataCriacao, DateTime? dataAtualizacao, DateTime? dataFinalizacao, double valor, double desconto, double acrescimo)
    {
        Id = id;
        Descricao = descricao;
        CodigoAcompanhamento = codigoAcompanhamento;
        Status = status;
        ClienteResponsavelId = clienteResponsavelId;
        FuncionarioResponsavelId = funcionarioResponsavelId;
        VeiculoId = veiculoId;
        DataCriacao = dataCriacao;
        DataAtualizacao = dataAtualizacao;
        DataFinalizacao = dataFinalizacao;
        Valor = valor;
        Desconto = desconto;
        Acrescimo = acrescimo;
    }

    public void TransicionarPara(StatusOS novoStatus)
    {
        if (!Transicoes.TryGetValue(Status, out var permitidos) || !permitidos.Contains(novoStatus))
            throw new InvalidOperationException($"Transição inválida: {Status} -> {novoStatus}.");

        var statusAnterior = Status;
        var agora = DateTime.UtcNow;

        Status = novoStatus;
        DataAtualizacao = agora;

        _eventosDominio.Add(new StatusOrdemServicoAlteradoEvent(
            Guid.NewGuid(),
            Id,
            ClienteResponsavelId,
            CodigoAcompanhamento,
            statusAnterior,
            novoStatus,
            agora));

        if (novoStatus == StatusOS.Finalizada)
            DataFinalizacao = agora;
    }

    public bool ReceberDecisaoExterna(
        string eventoId,
        DecisaoOrcamento decisao,
        string? motivo,
        DateTime ocorridoEm,
        DateTime recebidoEm)
    {
        var eventoNormalizado = eventoId.Trim();
        var existente = DecisoesExternas.FirstOrDefault(item =>
            string.Equals(item.EventoId, eventoNormalizado, StringComparison.Ordinal));

        if (existente is not null)
        {
            if (existente.CorrespondeA(decisao, motivo, ocorridoEm))
                return false;

            throw new DomainConflictException(
                $"O evento externo {eventoNormalizado} já foi registrado com outro conteúdo.");
        }

        if (Status != StatusOS.AguardandoAprovacao)
        {
            throw new DomainConflictException(
                $"A OS {Id} não aguarda decisão de orçamento. Status atual: {Status}.");
        }

        var novoStatus = decisao switch
        {
            DecisaoOrcamento.Aprovado => StatusOS.EmExecucao,
            DecisaoOrcamento.Recusado => StatusOS.Reprovada,
            _ => throw new ArgumentOutOfRangeException(nameof(decisao), decisao, "Decisão inválida.")
        };

        DecisoesExternas.Add(new DecisaoOrcamentoExterna(
            Guid.NewGuid(),
            Id,
            eventoNormalizado,
            decisao,
            DecisaoOrcamentoExterna.NormalizarMotivo(motivo),
            ocorridoEm,
            recebidoEm));

        TransicionarPara(novoStatus);
        return true;
    }

    public void LimparEventosDominio() => _eventosDominio.Clear();

    public void AtribuirFuncionario(Guid id)
    {
        FuncionarioResponsavelId = id;
    }

    public void AtribuirFuncionario(Funcionario funcionario)
    {
        FuncionarioResponsavel = funcionario;
        FuncionarioResponsavelId = funcionario.Id;
    }

    public void AtribuirVeiculo(Veiculo veiculo)
    {
        Veiculo = veiculo;
        VeiculoId = veiculo.Id;
    }

    public void AdicionarProdutos(OrdemServicoProdutos produto)
    {
        Produtos.Add(produto);
    }

    public void AdicionarServicos(OrdemServicoServicos servico)
    {
        Servicos.Add(servico);
    }

    public void AtualizarData(DateTime dataAtualizacao)
    {
        DataAtualizacao = dataAtualizacao;
    }

    public void AtribuirValor(double valor)
    {
        Valor = valor;
    }
}
