using TechChallenge.Domain.Entities;
using TechChallenge.Application.Abstractions.Repositories;

namespace TechChallenge.Application.Features.Funcionarios.ListarFuncionarios;

public class ListarFuncionariosService
{
    private readonly IFuncionarioRepository _funcionarioRepository;

    public ListarFuncionariosService(IFuncionarioRepository funcionarioRepository)
    {
        _funcionarioRepository = funcionarioRepository;
    }

    public async Task<List<Funcionario>> ListarFuncionarios(ListarFuncionariosQuery query)
    {
        return await _funcionarioRepository.GetAllAsync();
    }
}
