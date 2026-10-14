namespace TechChallenge.Domain.Entities
{
    public class CategoriaVeiculo
    {
        public Guid Id { get; private set; }
        public string Descricao { get; private set; }

        public CategoriaVeiculo(Guid id, string descricao)
        {
            Id = id;
            Descricao = descricao;
        }
    }
}