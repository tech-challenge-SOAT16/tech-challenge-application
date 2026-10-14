using TechChallenge.Application.Abstractions.Repositories;

namespace TechChallenge.Application.Features.Usuarios.DesvincularFuncionario;

public class DesvincularFuncionarioService
{
    private readonly IUsuarioRepository _usuarioRepository;

    public DesvincularFuncionarioService(IUsuarioRepository usuarioRepository)
    {
        _usuarioRepository = usuarioRepository;
    }

    public async Task DesvincularFuncionario(DesvincularFuncionarioCommand command)
    {
        var usuario = await _usuarioRepository.GetByIdAsync(command.UsuarioId);
        if (usuario is null)
            throw new KeyNotFoundException($"Usuário com Id {command.UsuarioId} não encontrado.");

        usuario = new Domain.Entities.Usuario(usuario.Id, usuario.Login, usuario.PasswordHash, usuario.TipoUsuario, usuario.Ativo, null);
        await _usuarioRepository.UpdateAsync(usuario);
    }
}
