using System;
namespace TechChallenge.Domain.Entities
{
    public class OrdemServicoServicos
    {
        public Guid Id { get; private set; }
        public Guid OrdemServicoId { get; private set; }
        public OrdemServico OrdemServico { get; private set; } = null!;
        public Guid ServicoId { get; private set; }
        public Servico Servico { get; private set; } = null!;
        public double Valor { get; private set; }
        public int Quantidade { get; private set; } = 1;
        public double Desconto { get; private set; }
        public double Acrescimo { get; private set; }

        public OrdemServicoServicos(Guid id, Guid ordemServicoId, Guid servicoId, double valor, int quantidade, double desconto, double acrescimo)
        {
            Id = id;
            OrdemServicoId = ordemServicoId;
            ServicoId = servicoId;
            Valor = valor;
            Quantidade = quantidade;
            Desconto = desconto;
            Acrescimo = acrescimo;
        }
    }
}
