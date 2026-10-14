using TechChallenge.Domain.Enums;

namespace TechChallenge.Domain.Entities;

public class Usuario
{
    public Guid Id { get; private set; }
    public string Login { get; private set; } = string.Empty;
    public string PasswordHash { get; private set; } = string.Empty;
    public TipoUsuario TipoUsuario { get; private set; }
    public bool Ativo { get; private set; } = true;

    // Vínculo opcional com Funcionario (regra de dono-do-recurso). Nullable: Usuario pode existir sem Funcionario.
    public Guid? FuncionarioId { get; private set; }
    public Funcionario? Funcionario { get; private set; }

    public ICollection<RefreshToken> RefreshTokens { get; private set; } = new List<RefreshToken>();

    public Usuario(Guid id, string login, string passwordHash, TipoUsuario tipoUsuario, bool ativo, Guid? funcionarioId = null)
    {
        Id = id;
        Login = login;
        PasswordHash = passwordHash;
        TipoUsuario = tipoUsuario;
        Ativo = ativo;
        FuncionarioId = funcionarioId;
    }
}