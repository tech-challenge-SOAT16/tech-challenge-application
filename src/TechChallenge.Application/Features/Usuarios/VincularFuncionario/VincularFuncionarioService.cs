using TechChallenge.Application.Abstractions.Repositories;

namespace TechChallenge.Application.Features.Usuarios.VincularFuncionario;

public class VincularFuncionarioService
{
    private readonly IUsuarioRepository _usuarioRepository;
    private readonly IFuncionarioRepository _funcionarioRepository;

    public VincularFuncionarioService(
        IUsuarioRepository usuarioRepository,
        IFuncionarioRepository funcionarioRepository)
    {
        _usuarioRepository = usuarioRepository;
        _funcionarioRepository = funcionarioRepository;
    }

    public async Task VincularFuncionario(VincularFuncionarioCommand command)
    {
        var usuario = await _usuarioRepository.GetByIdAsync(command.UsuarioId);
        if (usuario is null)
            throw new KeyNotFoundException($"Usuário com Id {command.UsuarioId} não encontrado.");

        var funcionario = await _funcionarioRepository.GetByIdAsync(command.FuncionarioId);
        if (funcionario is null)
            throw new KeyNotFoundException($"Funcionário com Id {command.FuncionarioId} não encontrado.");

        var jaVinculado = await _usuarioRepository.ExisteVinculoFuncionarioAsync(command.FuncionarioId);
        if (jaVinculado && usuario.FuncionarioId != command.FuncionarioId)
            throw new InvalidOperationException($"O funcionário {command.FuncionarioId} já está vinculado a outro usuário.");

        usuario = new Domain.Entities.Usuario(usuario.Id, usuario.Login, usuario.PasswordHash, usuario.TipoUsuario, usuario.Ativo, command.FuncionarioId);

        await _usuarioRepository.UpdateAsync(usuario);
    }
}
