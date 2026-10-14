using System;
namespace TechChallenge.Domain.Entities
{
    public class OrdemServicoProdutos
    {
        public Guid Id { get; private set; }
        public Guid OrdemServicoId { get; private set; }
        public OrdemServico OrdemServico { get; private set; } = null!;
        public Guid ProdutoId { get; private set; }
        public Produto Produto { get; private set; } = null!;
        public double Valor { get; private set; }
        public int Quantidade { get; private set; } = 1;
        public double Desconto { get; private set; }
        public double Acrescimo { get; private set; }

        public OrdemServicoProdutos(Guid id, Guid ordemServicoId, Guid produtoId, double valor, int quantidade, double desconto, double acrescimo)
        {
            Id = id;
            OrdemServicoId = ordemServicoId;
            ProdutoId = produtoId;
            Valor = valor;
            Quantidade = quantidade;
            Desconto = desconto;
            Acrescimo = acrescimo;
        }

        public void AdicionarProduto(Guid id, string descricao, decimal valor, Guid categoria)
        {
            Produto = new Produto(id, descricao, valor, categoria);
        }
    }
}