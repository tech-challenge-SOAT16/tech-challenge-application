using TechChallenge.Application.Abstractions.Repositories;
using TechChallenge.Domain.Entities;

namespace TechChallenge.Application.Features.Usuarios.ObterUsuario;

public class ObterUsuarioService
{
    private readonly IUsuarioRepository _usuarioRepository;

    public ObterUsuarioService(IUsuarioRepository usuarioRepository)
    {
        _usuarioRepository = usuarioRepository;
    }

    public async Task<Usuario> ObterUsuario(ObterUsuarioQuery query)
    {
        var usuario = await _usuarioRepository.GetByIdAsync(query.Id);
        if (usuario is null)
            throw new KeyNotFoundException($"Usuário com Id {query.Id} não encontrado.");

        return usuario;
    }
}
