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
using TechChallenge.Infrastructure.Observability;

var builder = WebApplication.CreateBuilder(args);
builder.AddObservability();

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

// No laboratório, o processo só começa a servir HTTP após migrations e seed.
// Se a inicialização falhar, o processo termina e o Kubernetes tenta novamente.
if (!app.Environment.IsEnvironment("Testing"))
{
    using (var scope = app.Services.CreateScope())
    {
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        await context.Database.MigrateAsync();
    }
    await DataSeeder.SeedAsync(app.Services);
}

app.UseCorrelationId();
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

public partial class Program
{
    protected Program()
    {
    }
}
