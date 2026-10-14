namespace TechChallenge.Domain.Entities
{
    public class CategoriaProduto
    {
        public Guid Id { get; private set; }
        public string Descricao { get; private set; }

        public CategoriaProduto(Guid id, string descricao)
        {
            Id = id;
            Descricao = descricao;
        }
    }
}