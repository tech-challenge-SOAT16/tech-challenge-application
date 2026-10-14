using TechChallenge.Application.Abstractions.Repositories;
using TechChallenge.Domain.Entities;

namespace TechChallenge.Application.Features.Usuarios.ListarUsuarios;

public class ListarUsuariosService
{
    private readonly IUsuarioRepository _usuarioRepository;

    public ListarUsuariosService(IUsuarioRepository usuarioRepository)
    {
        _usuarioRepository = usuarioRepository;
    }

    public async Task<List<Usuario>> ListarUsuarios()
    {
        return await _usuarioRepository.GetAllAsync();
    }
}
