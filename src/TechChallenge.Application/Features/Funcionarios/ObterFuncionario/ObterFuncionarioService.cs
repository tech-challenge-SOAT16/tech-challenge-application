using TechChallenge.Domain.Entities;
using TechChallenge.Application.Abstractions.Repositories;

namespace TechChallenge.Application.Features.Funcionarios.ObterFuncionario;

public class ObterFuncionarioService
{
    private readonly IFuncionarioRepository _funcionarioRepository;

    public ObterFuncionarioService(IFuncionarioRepository funcionarioRepository)
    {
        _funcionarioRepository = funcionarioRepository;
    }

    public async Task<Funcionario> ObterFuncionario(ObterFuncionarioQuery query)
    {
        var funcionario = await _funcionarioRepository.GetByIdAsync(query.Id);
        if (funcionario is null)
            throw new KeyNotFoundException($"Funcionário com Id {query.Id} não encontrado.");

        return funcionario;
    }
}
