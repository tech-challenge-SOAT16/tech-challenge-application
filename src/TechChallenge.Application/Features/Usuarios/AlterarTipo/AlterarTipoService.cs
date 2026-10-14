using TechChallenge.Application.Abstractions.Repositories;

namespace TechChallenge.Application.Features.Usuarios.AlterarTipo;

public class AlterarTipoService
{
    private readonly IUsuarioRepository _usuarioRepository;

    public AlterarTipoService(IUsuarioRepository usuarioRepository)
    {
        _usuarioRepository = usuarioRepository;
    }

    public async Task AlterarTipo(AlterarTipoCommand command)
    {
        var usuario = await _usuarioRepository.GetByIdAsync(command.UsuarioId);
        if (usuario is null)
            throw new KeyNotFoundException($"Usuário com Id {command.UsuarioId} não encontrado.");

        usuario = new Domain.Entities.Usuario(usuario.Id, usuario.Login, usuario.PasswordHash, command.TipoUsuario, usuario.Ativo, usuario.FuncionarioId);
        await _usuarioRepository.UpdateAsync(usuario);
    }
}
