using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using TechChallenge.Application.Abstractions.Auth;
using TechChallenge.Domain.Entities;
using TechChallenge.Domain.Enums;
using TechChallenge.Infrastructure.Database.Context;

namespace TechChallenge.Infrastructure.Database.Seeding;

internal static class DemoSeeder
{
    private static readonly Guid ClienteId = Guid.Parse("aaaaaaaa-0000-0000-0000-000000000001");
    private static readonly Guid EnderecoClienteId = Guid.Parse("aaaaaaaa-0000-0000-0000-000000000002");
    private static readonly Guid VendedorId = Guid.Parse("aaaaaaaa-0000-0000-0000-000000000003");
    private static readonly Guid EnderecoVendedorId = Guid.Parse("aaaaaaaa-0000-0000-0000-000000000004");
    private static readonly Guid MecanicoId = Guid.Parse("aaaaaaaa-0000-0000-0000-000000000005");
    private static readonly Guid EnderecoMecanicoId = Guid.Parse("aaaaaaaa-0000-0000-0000-000000000006");
    private static readonly Guid AdminId = Guid.Parse("aaaaaaaa-0000-0000-0000-000000000007");
    private static readonly Guid VeiculoId = Guid.Parse("aaaaaaaa-0000-0000-0000-000000000008");
    private static readonly Guid ServicoRevisaoId = Guid.Parse("aaaaaaaa-0000-0000-0000-000000000009");
    private static readonly Guid ServicoDiagnosticoId = Guid.Parse("aaaaaaaa-0000-0000-0000-000000000010");
    private static readonly Guid ProdutoFiltroId = Guid.Parse("aaaaaaaa-0000-0000-0000-000000000011");
    private static readonly Guid ProdutoPastilhaId = Guid.Parse("aaaaaaaa-0000-0000-0000-000000000012");
    private static readonly Guid UsuarioAdminId = Guid.Parse("aaaaaaaa-0000-0000-0000-000000000013");
    private static readonly Guid UsuarioVendedorId = Guid.Parse("aaaaaaaa-0000-0000-0000-000000000014");
    private static readonly Guid UsuarioMecanicoId = Guid.Parse("aaaaaaaa-0000-0000-0000-000000000015");
    private static readonly Guid CategoriaProdutoFiltroId = Guid.Parse("aaaaaaaa-0000-0000-0000-000000000016");
    private static readonly Guid CategoriaProdutoPastilhaId = Guid.Parse("aaaaaaaa-0000-0000-0000-000000000017");
    private static readonly Guid CategoriaServicoRevisaoId = Guid.Parse("aaaaaaaa-0000-0000-0000-000000000018");
    private static readonly Guid CategoriaServicoDiagnosticoId = Guid.Parse("aaaaaaaa-0000-0000-0000-000000000019");
    private static readonly Guid CategoriaVeiculoId = Guid.Parse("aaaaaaaa-0000-0000-0000-000000000020");
    private static readonly Guid EstoqueFiltroId = Guid.Parse("aaaaaaaa-0000-0000-0000-000000000021");
    private static readonly Guid EstoquePastilhaId = Guid.Parse("aaaaaaaa-0000-0000-0000-000000000022");

    public static async Task SeedAsync(IServiceProvider provider, ApplicationDbContext context)
    {
        var hasher = provider.GetRequiredService<IPasswordHasher>();

        await AddRangeIfMissingAsync(context, context.Endereco, Enderecos());
        await AddClienteIfMissingAsync(context);
        await AddRangeIfMissingAsync(context, context.Funcionario, Funcionarios());
        await AddRangeIfMissingAsync(context, context.CategoriaProduto, CategoriaProdutos());
        await AddRangeIfMissingAsync(context, context.CategoriaServico, CategoriaServicos());
        await AddRangeIfMissingAsync(context, context.CategoriaVeiculo, CategoriaVeiculos());
        await AddRangeIfMissingAsync(context, context.Veiculo, Veiculos());
        await AddRangeIfMissingAsync(context, context.Servico, Servicos());
        await AddRangeIfMissingAsync(context, context.Produto, Produtos());
        await AddUsuariosIfMissingAsync(context, hasher);
        await AddRangeIfMissingAsync(context, context.Estoque, Estoque());

        await context.SaveChangesAsync();
    }

    private static IReadOnlyCollection<Endereco> Enderecos() =>
    [
        new(EnderecoClienteId, "Rua Demo Cliente", "Apto 101", "100", "Centro", "Sao Paulo", "SP", "01001000"),
        new(EnderecoVendedorId, "Rua Demo Oficina", "Balcao", "200", "Oficinas", "Sao Paulo", "SP", "02002000"),
        new(EnderecoMecanicoId, "Rua Demo Oficina", "Box 3", "200", "Oficinas", "Sao Paulo", "SP", "02002000")
    ];

    private static Cliente Cliente() => new(
        ClienteId,
        "Cliente Demo",
        "cliente.demo@oficina.local",
        TipoDocumento.Cpf,
        "12345678909",
        EnderecoClienteId);

    private static IReadOnlyCollection<Funcionario> Funcionarios() =>
    [
        new(VendedorId, "Vendedor Demo", "12345678909", "111111111", TipoFuncionario.Vendedor, EnderecoVendedorId),
        new(MecanicoId, "Mecanico Demo", "98765432100", "222222222", TipoFuncionario.Mecanico, EnderecoMecanicoId),
        new(AdminId, "Administrador Demo", "11144477735", "333333333", TipoFuncionario.Administrador, EnderecoVendedorId)
    ];

    private static IReadOnlyCollection<CategoriaProduto> CategoriaProdutos() =>
    [
        new (CategoriaProdutoFiltroId, "Filtros"),
        new (Guid.NewGuid(), "Suspensão"),
        new (CategoriaProdutoPastilhaId, "Freios"),
        new (Guid.NewGuid(), "Correias"),
        new (Guid.NewGuid(), "Kits"),
        new (Guid.NewGuid(), "Ignição"),
        new (Guid.NewGuid(), "Injeção"),
        new (Guid.NewGuid(), "Óleos e Lubrificantes"),
        new (Guid.NewGuid(), "Fluidos"),
        new (Guid.NewGuid(), "Adivitos")
    ];

    private static IReadOnlyCollection<CategoriaServico> CategoriaServicos() =>
    [
        new (Guid.NewGuid(), "Troca de óleo do motor e filtros"),
        new (Guid.NewGuid(), "Substituição de fluidos (freio, câmbio e arrefecimento)"),
        new (Guid.NewGuid(), "Troca de velas, cabos e correias"),
        new (CategoriaServicoRevisaoId, "Revisão periódica geral"),
        new (Guid.NewGuid(), "Reparo e troca de pastilhas e discos de freio"),
        new (Guid.NewGuid(), "Manutenção da suspensão (amortecedores e molas)"),
        new (Guid.NewGuid(), "Alinhamento de direção e balanceamento de rodas"),
        new (Guid.NewGuid(), "Geometria e cambagem"),
        new (Guid.NewGuid(), "Retífica e reparo de motores"),
        new (Guid.NewGuid(), "Manutenção de caixas de câmbio (manual e automático)"),
        new (Guid.NewGuid(), "Sistema de injeção eletrônica e limpeza de bicos"),
        new (Guid.NewGuid(), "Sistema de escapamento"),
        new (Guid.NewGuid(), "Diagnóstico com scanner automotivo"),
        new (Guid.NewGuid(), "Troca de bateria e reparo no alternador/motor de partida"),
        new (Guid.NewGuid(), "Manutenção de ar-condicionado automotivo"),
        new (Guid.NewGuid(), "Reparos em painel, som e iluminação"),
        new (Guid.NewGuid(), "Funilaria (lanternagem) para correção de amassados"),
        new (Guid.NewGuid(), "Pintura automotiva e polimento"),
        new (Guid.NewGuid(), "Higienização interna"),
        new (CategoriaServicoDiagnosticoId, "Diagnóstico elétrico")
    ];

    private static IReadOnlyCollection<CategoriaVeiculo> CategoriaVeiculos() =>
    [
        new (Guid.NewGuid(),"Moto"),
        new (Guid.NewGuid(),"Motoneta"),
        new (Guid.NewGuid(),"Triciclo"),
        new (CategoriaVeiculoId, "Automóvel"),
        new (Guid.NewGuid(), "Micro-ônibus"),
        new (Guid.NewGuid(), "Ônibus"),
        new (Guid.NewGuid(), "Caminhonete"),
        new (Guid.NewGuid(), "Caminhão"),
        new (Guid.NewGuid(), "Reboque"),
        new (Guid.NewGuid(), "Camioneta"),
        new (Guid.NewGuid(), "Utilitários"),
        new (Guid.NewGuid(), "Tratores"),
        new (Guid.NewGuid(), "Veículos de coleção"),
        new (Guid.NewGuid(), "Adaptados")
    ];

    private static IReadOnlyCollection<Veiculo> Veiculos() =>
    [
        new(VeiculoId, "DEM1O23", "Civic", "Honda", "Prata", 2020, 45200, 98000, ClienteId, CategoriaVeiculoId)
    ];

    private static IReadOnlyCollection<Servico> Servicos() =>
    [
        new(ServicoRevisaoId, "Revisão preventiva demo", 350, CategoriaServicoRevisaoId),
        new(ServicoDiagnosticoId, "Diagnóstico elétrico demo", 180, CategoriaServicoDiagnosticoId)
    ];

    private static IReadOnlyCollection<Produto> Produtos() =>
    [
        new Produto(ProdutoFiltroId, "Filtro de oleo demo", 65, CategoriaProdutoFiltroId),
        new Produto(ProdutoPastilhaId, "Pastilha de freio demo", 220, CategoriaProdutoPastilhaId)
    ];

    private static IReadOnlyCollection<Usuario> Usuarios(IPasswordHasher hasher) =>
    [
        new(UsuarioAdminId, "admin.demo", hasher.Hash("Demo@123"), TipoUsuario.Administrador, true, AdminId),
        new(UsuarioVendedorId, "vendedor.demo", hasher.Hash("Demo@123"), TipoUsuario.Vendedor, true, VendedorId),
        new(UsuarioMecanicoId, "mecanico.demo", hasher.Hash("Demo@123"), TipoUsuario.Mecanico, true, MecanicoId)
    ];

    private static async Task AddClienteIfMissingAsync(ApplicationDbContext context)
    {
        var cliente = Cliente();
        var existente = await context.Cliente.FirstOrDefaultAsync(c =>
            c.Id == cliente.Id || c.Documento == cliente.Documento);

        if (existente is null)
            context.Cliente.Add(cliente);
        else if (!string.Equals(existente.Email, cliente.Email, StringComparison.OrdinalIgnoreCase))
            existente.AtualizarEmail(cliente.Email);
    }

    private static async Task AddUsuariosIfMissingAsync(ApplicationDbContext context, IPasswordHasher hasher)
    {
        foreach (var usuario in Usuarios(hasher))
        {
            var exists = await context.Usuario.AnyAsync(u => u.Id == usuario.Id || u.Login == usuario.Login);

            if (!exists)
                context.Usuario.Add(usuario);
        }
    }

    private static IReadOnlyCollection<Estoque> Estoque() =>
    [
        new (EstoqueFiltroId, ProdutoFiltroId, 100),
        new (EstoquePastilhaId, ProdutoPastilhaId, 50)
    ];

    private static async Task AddRangeIfMissingAsync<TEntity>(
    ApplicationDbContext context,
    DbSet<TEntity> dbSet,
    IEnumerable<TEntity> entities)
    where TEntity : class
    {
        foreach (var entity in entities)
        {
            var id = context.Entry(entity).Property<Guid>("Id").CurrentValue;
            var exists = await dbSet.AnyAsync(e => EF.Property<Guid>(e, "Id") == id);

            if (!exists)
                dbSet.Add(entity);
        }
    }
}
