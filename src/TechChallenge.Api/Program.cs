using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi;
using System.Text.Json.Serialization;
using TechChallenge.Api.Endpoints;
using TechChallenge.Api.Configuration;
using TechChallenge.Api.Filters;
using TechChallenge.Api.HealthChecks;
using TechChallenge.Api.Middleware;
using TechChallenge.Infrastructure.Auth;
using TechChallenge.Infrastructure.Database.Context;
using TechChallenge.Infrastructure.Database.Seeding;
using TechChallenge.Infrastructure.IoC.Injection;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi(options =>
{
    options.AddDocumentTransformer((document, context, cancellationToken) =>
    {
        document.Components ??= new OpenApiComponents();
        document.Components.SecuritySchemes ??= new Dictionary<string, IOpenApiSecurityScheme>();
        document.Components.SecuritySchemes["Bearer"] = new OpenApiSecurityScheme
        {
            Type = SecuritySchemeType.Http,
            Scheme = "bearer",
            BearerFormat = "JWT",
            In = ParameterLocation.Header,
            Description = "Informe o token JWT."
        };

        document.Security =
        [
            new OpenApiSecurityRequirement
            {
                [new OpenApiSecuritySchemeReference("Bearer", document)] = new List<string>()
            }
        ];

        return Task.CompletedTask;
    });
});
builder.Services.AddSwaggerGen();
builder.Services.ConfigureHttpJsonOptions(options =>
{
    options.SerializerOptions.Converters.Add(new JsonStringEnumConverter());
});
builder.Services.Configure<IntegracaoExternaOptions>(
    builder.Configuration.GetSection(IntegracaoExternaOptions.SectionName));
builder.Services.AddScoped<IntegrationApiKeyFilter>();
builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddAuth(builder.Configuration, builder.Environment);
builder.Services.AddHealthChecks()
    .AddCheck<DatabaseHealthCheck>("database", tags: ["ready"], timeout: TimeSpan.FromSeconds(3));

var app = builder.Build();

// O processo só começa a servir HTTP após migrations e seed.
// Se a inicialização falhar, o processo termina e o Kubernetes tenta novamente.
// Como o HPA inicia com minReplicas >= 2, usamos um advisory lock PostgreSQL
// (pg_advisory_lock) para garantir que apenas uma réplica execute migrations
// e seed por vez; as demais ficam bloqueadas no lock até o término e, depois,
// apenas revalidam que não há migrations pendentes.
if (!app.Environment.IsEnvironment("Testing"))
{
    await ApplyMigrationsAndSeedAsync(app.Services);
}

app.UseMiddleware<ExceptionMiddleware>();

app.MapHealthChecks("/health/live", new HealthCheckOptions
{
    // Falha do banco retira o pod do Service, mas não deve reiniciar o processo.
    Predicate = _ => false
}).AllowAnonymous();
app.MapHealthChecks("/health/ready", new HealthCheckOptions
{
    Predicate = check => check.Tags.Contains("ready")
}).AllowAnonymous();

app.MapOpenApi().AllowAnonymous();

app.UseSwagger();
app.UseSwaggerUI(options =>
{
    options.SwaggerEndpoint("/openapi/v1.json", "TechChallenge API");
});
if (app.Configuration.GetValue("Http:UseHttpsRedirection", true))
{
    app.UseHttpsRedirection();
}

app.UseAuthentication();
app.UseAuthorization();

app.MapAuthEndpoints();
app.MapUsuariosEndpoints();
app.MapClientesEndpoints();
app.MapFuncionariosEndpoints();
app.MapVeiculosEndpoints();
app.MapServicosEndpoints();
app.MapInventarioEndpoints();
app.MapOrdensServicoEndpoints();
app.MapCategoriaServicoEndpoints();
app.MapCategoriaProdutoEndpoints();
app.MapCategoriaVeiculoEndpoints();
app.MapEstoqueEndpoints();
app.MapIntegracoesEndpoints();
app.MapDecisoesOrcamentoEmailEndpoints();

await app.RunAsync();

static async Task ApplyMigrationsAndSeedAsync(IServiceProvider services)
{
    // Chave arbitrária estável para o lock de migrations desta aplicação.
    // Qualquer bigint fixo serve, desde que não colida com outros locks na
    // mesma instância PostgreSQL. Documentar o valor evita colisões futuras.
    const long migrationsLockKey = 727292024L;

    using var scope = services.CreateScope();
    var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    var logger = scope.ServiceProvider
        .GetRequiredService<ILoggerFactory>()
        .CreateLogger("Startup.Migrations");

    var connection = context.Database.GetDbConnection();
    var openedHere = connection.State != System.Data.ConnectionState.Open;
    if (openedHere)
    {
        await connection.OpenAsync();
    }

    try
    {
        logger.LogInformation(
            "Aguardando advisory lock de migrations (chave {LockKey}).", migrationsLockKey);

        await using (var lockCommand = connection.CreateCommand())
        {
            lockCommand.CommandText = $"SELECT pg_advisory_lock({migrationsLockKey});";
            await lockCommand.ExecuteNonQueryAsync();
        }

        try
        {
            logger.LogInformation("Advisory lock adquirido. Aplicando migrations.");
            await context.Database.MigrateAsync();

            logger.LogInformation("Migrations concluídas. Executando seed.");
            await DataSeeder.SeedAsync(services);
        }
        finally
        {
            await using var unlockCommand = connection.CreateCommand();
            unlockCommand.CommandText = $"SELECT pg_advisory_unlock({migrationsLockKey});";
            await unlockCommand.ExecuteNonQueryAsync();
            logger.LogInformation("Advisory lock liberado.");
        }
    }
    finally
    {
        if (openedHere)
        {
            await connection.CloseAsync();
        }
    }
}

public partial class Program
{
    protected Program()
    {
    }
}
