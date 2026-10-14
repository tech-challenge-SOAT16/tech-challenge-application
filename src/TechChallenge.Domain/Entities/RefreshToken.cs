using System;
namespace TechChallenge.Domain.Entities;

public class RefreshToken
{
    public Guid Id { get; private set; }
    public Guid UsuarioId { get; private set; }
    public Usuario Usuario { get; private set; } = null!;

    public string TokenHash { get; private set; } = string.Empty;
    public DateTime CriadoEm { get; private set; }
    public DateTime ExpiraEm { get; private set; }
    public DateTime? RevogadoEm { get; private set; }

    public RefreshToken(Guid id, Guid usuarioId, string tokenHash, DateTime criadoEm, DateTime expiraEm, DateTime? revogadoEm = null)
    {
        Id = id;
        UsuarioId = usuarioId;
        TokenHash = tokenHash;
        CriadoEm = criadoEm;
        ExpiraEm = expiraEm;
        RevogadoEm = revogadoEm;
    }

    public bool EstaAtivo(DateTime agora) =>
        RevogadoEm is null
        && agora < ExpiraEm;

    public void AlterarRevogacao(DateTime revogadoEm)
    {
        RevogadoEm = revogadoEm;
    }

    public void AtribuirUsuario(Usuario usuario)
    {
        Usuario = usuario;
        UsuarioId = usuario.Id;
    }
}
