namespace TechChallenge.Domain.Entities
{
    public class CategoriaServico
    {
        public Guid Id { get; private set; }
        public string Descricao { get; private set; }

        public CategoriaServico(Guid id, string descricao)
        {
            Id = id;
            Descricao = descricao;
        }
    }
}