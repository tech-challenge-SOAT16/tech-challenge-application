using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using TechChallenge.Application.Abstractions.Auth;
using TechChallenge.Domain.Entities;
using TechChallenge.Domain.Enums;
using TechChallenge.Infrastructure.Database.Context;

namespace TechChallenge.Infrastructure.Database.Seeding
{
    internal static class FakeDataSeeder
    {
        private static readonly Guid ClienteExemploId = Guid.Parse("11111111-1111-1111-1111-111111111111");
        private static readonly Guid EnderecoClienteExemploId = Guid.Parse("22222222-2222-2222-2222-222222222222");
        private static readonly Guid FuncionarioExemploId = Guid.Parse("33333333-3333-3333-3333-333333333333");
        private static readonly Guid EnderecoFuncionarioExemploId = Guid.Parse("66666666-6666-6666-6666-666666666666");
        private static readonly Guid UsuarioExemploId = Guid.Parse("44444444-4444-4444-4444-444444444444");
        private static readonly Guid VeiculoExemploId = Guid.Parse("55555555-5555-5555-5555-555555555555");
        private static readonly Guid CategoriaVeiculoId = Guid.Parse("88888888-8888-8888-8888-888888888888");

        public static async Task SeedAsync(IServiceProvider provider, ApplicationDbContext context)
        {
            var hasher = provider.GetRequiredService<IPasswordHasher>();

            await AddRangeIfMissingAsync(context, context.Endereco, Enderecos());
            await AddClientesIfMissingAsync(context);
            await AddFuncionariosIfMissingAsync(context);
            await AddUsuariosIfMissingAsync(context, hasher);
            await AddRangeIfMissingAsync(context, context.CategoriaVeiculo, CategoriaVeiculos());
            await AddVeiculosIfMissingAsync(context);
            await AddRangeIfMissingAsync(context, context.CategoriaServico, CategoriaServicos());
            await AddRangeIfMissingAsync(context, context.CategoriaProduto, CategoriaProdutos());
            await AddRangeIfMissingAsync(context, context.Servico, Servicos());
            await AddRangeIfMissingAsync(context, context.Produto, Produtos());
            await AddRangeIfMissingAsync(context, context.OrdemServico, OrdensServico());

            await context.SaveChangesAsync();
        }

        private static IReadOnlyCollection<Endereco> Enderecos() =>
        [
            new(EnderecoClienteExemploId, "Rua Exemplo", "Casa", "123", "Centro", "Sao Paulo", "SP", "01001000"),
            new(EnderecoFuncionarioExemploId, "Rua Oficina", "Sala 2", "456", "Centro", "Sao Paulo", "SP", "02002000")
        ];

        private static IReadOnlyCollection<Cliente> Clientes() =>
        [
            new(ClienteExemploId, "Cliente Fake", "cliente.fake@oficina.local", TipoDocumento.Rg, "000000000", EnderecoClienteExemploId)
        ];

        private static IReadOnlyCollection<Funcionario> Funcionarios() =>
        [
            new(FuncionarioExemploId, "Funcionario Fake", "00000000000", "000000000", TipoFuncionario.Mecanico, EnderecoFuncionarioExemploId)
        ];

        private static IReadOnlyCollection<Usuario> Usuarios(IPasswordHasher hasher) =>
        [
            new(UsuarioExemploId, "mecanico.fake", hasher.Hash("Senha@123"), TipoUsuario.Mecanico, true, FuncionarioExemploId)
        ];

        private static IReadOnlyCollection<CategoriaServico> CategoriaServicos() => [];

        private static IReadOnlyCollection<CategoriaProduto> CategoriaProdutos() => [];

        private static IReadOnlyCollection<CategoriaVeiculo> CategoriaVeiculos() =>
        [
            new(CategoriaVeiculoId, "Categoria Fake")
        ];

        private static IReadOnlyCollection<Veiculo> Veiculos() =>
        [
            new(VeiculoExemploId, "AAA1111", "Modelo Fake", "Marca Fake", "Cor Fake", 2022, 0, 0, ClienteExemploId, CategoriaVeiculoId)
        ];

        private static IReadOnlyCollection<Servico> Servicos() => [];

        private static IReadOnlyCollection<Produto> Produtos() => [];

        private static IReadOnlyCollection<OrdemServico> OrdensServico() => [];

        private static async Task AddClientesIfMissingAsync(ApplicationDbContext context)
        {
            foreach (var cliente in Clientes())
            {
                var exists = await context.Cliente.AnyAsync(c => c.Id == cliente.Id || c.Documento == cliente.Documento);

                if (!exists)
                    context.Cliente.Add(cliente);
            }
        }

        private static async Task AddFuncionariosIfMissingAsync(ApplicationDbContext context)
        {
            foreach (var funcionario in Funcionarios())
            {
                var exists = await context.Funcionario.AnyAsync(f => f.Id == funcionario.Id || f.Cpf == funcionario.Cpf);

                if (!exists)
                    context.Funcionario.Add(funcionario);
            }
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

        private static async Task AddVeiculosIfMissingAsync(ApplicationDbContext context)
        {
            foreach (var veiculo in Veiculos())
            {
                var exists = await context.Veiculo.AnyAsync(v => v.Id == veiculo.Id || v.Placa == veiculo.Placa);

                if (!exists)
                    context.Veiculo.Add(veiculo);
            }
        }

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
}
