using System;
using FluentValidation;
using TechChallenge.Domain.Entities;
using TechChallenge.Domain.Enums;
using TechChallenge.Application.Abstractions.Notifications;
using TechChallenge.Application.Abstractions.Repositories;
using TechChallenge.Application.Notifications;

namespace TechChallenge.Application.Features.OS.CriarOS;

public class CriarOSService
{
    private readonly IOrdemServicoRepository _ordemServicoRepository;
    private readonly IClienteRepository _clienteRepository;
    private readonly IFuncionarioRepository _funcionarioRepository;
    private readonly IVeiculoRepository _veiculoRepository;
    private readonly IServicoRepository _servicoRepository;
    private readonly IProdutoRepository _produtoRepository;
    private readonly IEstoqueRepository _estoqueRepository;
    private readonly IValidator<CriarOSCommand> _validator;
    private readonly INotificationService _notificationService;

    public CriarOSService(
        IOrdemServicoRepository ordemServicoRepository,
        IClienteRepository clienteRepository,
        IFuncionarioRepository funcionarioRepository,
        IVeiculoRepository veiculoRepository,
        IServicoRepository servicoRepository,
        IProdutoRepository produtoRepository,
        IEstoqueRepository estoqueRepository,
        IValidator<CriarOSCommand> validator,
        INotificationService? notificationService = null)
    {
        _ordemServicoRepository = ordemServicoRepository;
        _clienteRepository = clienteRepository;
        _funcionarioRepository = funcionarioRepository;
        _veiculoRepository = veiculoRepository;
        _servicoRepository = servicoRepository;
        _produtoRepository = produtoRepository;
        _estoqueRepository = estoqueRepository;
        _validator = validator;
        _notificationService = notificationService ?? NullNotificationService.Instance;
    }

    public async Task<Guid> CriarOSAsync(CriarOSCommand command)
    {
        _validator.ValidateAndThrow(command);

        var cliente = await _clienteRepository.GetByIdAsync(command.ClienteResponsavelId);
        if (cliente is null)
            throw new KeyNotFoundException($"Cliente com Id {command.ClienteResponsavelId} não encontrado.");

        var funcionario = await _funcionarioRepository.GetByIdAsync(command.FuncionarioResponsavelId);
        if (funcionario is null)
            throw new KeyNotFoundException($"Funcionário com Id {command.FuncionarioResponsavelId} não encontrado.");

        var veiculo = await _veiculoRepository.GetByIdAsync(command.VeiculoId);
        if (veiculo is null)
            throw new KeyNotFoundException($"Veículo com Id {command.VeiculoId} não encontrado.");

        var os = new OrdemServico(Guid.NewGuid(), command.Descricao, Guid.NewGuid().ToString("N"), StatusOS.Recebida, command.ClienteResponsavelId, command.FuncionarioResponsavelId, command.VeiculoId, DateTime.UtcNow, null, null, valor: 0, desconto: 0, acrescimo: 0);

        foreach (var item in command.Servicos)
        {
            var servico = await _servicoRepository.GetByIdAsync(item.Id);
            if (servico is null)
                throw new KeyNotFoundException($"Serviço com Id {item.Id} não encontrado.");

            os.AdicionarServicos(new OrdemServicoServicos(
                Guid.NewGuid(), os.Id, servico.Id, (double)servico.Valor, item.Quantidade, 0, 0));
        }

        foreach (var item in command.Produtos)
        {
            var produto = await _produtoRepository.GetByIdAsync(item.Id);
            if (produto is null)
                throw new KeyNotFoundException($"Produto com Id {item.Id} não encontrado.");

            var estoque = await _estoqueRepository.GetByIdProdutoAsync(item.Id);
            if (estoque is null)
                throw new InvalidOperationException($"Não foi encontrado estoque lançado para o produto {produto.Descricao}.");

            if (estoque.Quantidade < item.Quantidade)
                throw new InvalidOperationException($"Estoque insuficiente para o produto {produto.Descricao}.");

            os.AdicionarProdutos(new OrdemServicoProdutos(
                Guid.NewGuid(), os.Id, produto.Id, (double)produto.Valor, item.Quantidade, 0, 0));
        }

        await _ordemServicoRepository.AddAsync(os);
        _notificationService.NotificarFuncionariosPorFuncao(
            TipoFuncionario.Mecanico,
            "Nova OS na fila",
            $"OS {os.Id} recebida para diagnóstico.");

        return os.Id;
    }
}
