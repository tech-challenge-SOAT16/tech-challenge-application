using System.Data.Common;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using TechChallenge.Infrastructure.Database.Context;

namespace TechChallenge.IntegrationTests.Integration.Factories;

public class WebAplicationFactory<TProgram> : WebApplicationFactory<TProgram> where TProgram : class
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureAppConfiguration(config =>
        {
            config.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Jwt:EnableLocalIssuance"] = "true",
                ["Jwt:LocalSigningKey"] = "test-local-key-com-mais-de-32-caracteres-1234567890",
                ["Jwt:Issuer"] = "techchallenge-api",
                ["Jwt:Audience"] = "techchallenge-clients",
                ["ApprovalLinks:SigningKey"] = "test-approval-key-com-mais-de-32-caracteres-123456",
                ["IntegracaoExterna:ApiKey"] = "integration-test-key",
                ["Notifications:Enabled"] = "false"
            });
        });

        builder.ConfigureServices(services =>
        {
            SQLitePCL.Batteries.Init();

            // Substitui o JWT Bearer pelo esquema de teste (autentica como Administrador por padrão).
            services.AddAuthentication(TestAuthHandler.SchemeName)
                .AddScheme<AuthenticationSchemeOptions, TestAuthHandler>(TestAuthHandler.SchemeName, _ => { });

            // busca o context na aplicação
            var dbContextDescriptor = services.SingleOrDefault(
                d => d.ServiceType ==
                     typeof(IDbContextOptionsConfiguration<ApplicationDbContext>));

            // remove o context da aplicação
            if (dbContextDescriptor is not null)
            {
                services.Remove(dbContextDescriptor);
            }

            services.RemoveAll<DbContextOptions<ApplicationDbContext>>();
            services.RemoveAll<IDbContextOptionsConfiguration<ApplicationDbContext>>();

            // Create open SqliteConnection so EF won't automatically close it.
            var connection = new SqliteConnection("DataSource=:memory:");
            connection.Open();
            services.AddSingleton<DbConnection>(connection);

            services.AddDbContext<ApplicationDbContext>((container, options) =>
            {
                var dbConnection = container.GetRequiredService<DbConnection>();
                options.UseSqlite(dbConnection);
            });

            var provider = services.BuildServiceProvider();
            using var scope = provider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            context.Database.EnsureDeleted();
            context.Database.EnsureCreated();
        });

        builder.UseEnvironment("Testing");
    }
}
