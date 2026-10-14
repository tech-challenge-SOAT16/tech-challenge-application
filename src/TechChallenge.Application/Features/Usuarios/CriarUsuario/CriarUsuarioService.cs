using System;
using FluentValidation;
using TechChallenge.Application.Abstractions.Auth;
using TechChallenge.Application.Abstractions.Repositories;
using TechChallenge.Domain.Entities;

namespace TechChallenge.Application.Features.Usuarios.CriarUsuario;

public class CriarUsuarioService
{
    private readonly IUsuarioRepository _usuarioRepository;
    private readonly IFuncionarioRepository _funcionarioRepository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IValidator<CriarUsuarioCommand> _validator;

    public CriarUsuarioService(
        IUsuarioRepository usuarioRepository,
        IFuncionarioRepository funcionarioRepository,
        IPasswordHasher passwordHasher,
        IValidator<CriarUsuarioCommand> validator)
    {
        _usuarioRepository = usuarioRepository;
        _funcionarioRepository = funcionarioRepository;
        _passwordHasher = passwordHasher;
        _validator = validator;
    }

    public async Task<Guid> CriarUsuario(CriarUsuarioCommand command)
    {
        _validator.ValidateAndThrow(command);

        var loginExiste = await _usuarioRepository.ExisteLoginAsync(command.Login);
        if (loginExiste)
            throw new InvalidOperationException($"Já existe um usuário com o login '{command.Login}'.");

        if (command.FuncionarioId is { } funcionarioId)
            await ValidarVinculo(funcionarioId);

        var usuario = new Usuario(Guid.NewGuid(), command.Login, _passwordHasher.Hash(command.Senha), command.TipoUsuario, true, command.FuncionarioId);

        await _usuarioRepository.AddAsync(usuario);
        return usuario.Id;
    }

    private async Task ValidarVinculo(Guid funcionarioId)
    {
        var funcionario = await _funcionarioRepository.GetByIdAsync(funcionarioId);
        if (funcionario is null)
            throw new KeyNotFoundException($"Funcionário com Id {funcionarioId} não encontrado.");

        var jaVinculado = await _usuarioRepository.ExisteVinculoFuncionarioAsync(funcionarioId);
        if (jaVinculado)
            throw new InvalidOperationException($"O funcionário {funcionarioId} já está vinculado a outro usuário.");
    }
}
