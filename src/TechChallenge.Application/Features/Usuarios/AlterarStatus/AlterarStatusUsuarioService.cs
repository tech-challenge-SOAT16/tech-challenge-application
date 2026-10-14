using System;
using TechChallenge.Application.Abstractions.Repositories;

namespace TechChallenge.Application.Features.Usuarios.AlterarStatus;

public class AlterarStatusUsuarioService
{
    private readonly IUsuarioRepository _usuarioRepository;
    private readonly IRefreshTokenRepository _refreshTokenRepository;

    public AlterarStatusUsuarioService(
        IUsuarioRepository usuarioRepository,
        IRefreshTokenRepository refreshTokenRepository)
    {
        _usuarioRepository = usuarioRepository;
        _refreshTokenRepository = refreshTokenRepository;
    }

    public async Task AlterarStatus(AlterarStatusUsuarioCommand command)
    {
        var usuario = await _usuarioRepository.GetByIdAsync(command.UsuarioId);
        if (usuario is null)
            throw new KeyNotFoundException($"Usuário com Id {command.UsuarioId} não encontrado.");

        usuario = new Domain.Entities.Usuario(usuario.Id, usuario.Login, usuario.PasswordHash, usuario.TipoUsuario, command.Ativo, usuario.FuncionarioId);
        await _usuarioRepository.UpdateAsync(usuario);

        // Desativar derruba todos os refresh tokens ativos.
        if (!command.Ativo)
        {
            await _refreshTokenRepository.RevogarTodasDoUsuarioAsync(usuario.Id, DateTime.UtcNow);
        }
    }
}
