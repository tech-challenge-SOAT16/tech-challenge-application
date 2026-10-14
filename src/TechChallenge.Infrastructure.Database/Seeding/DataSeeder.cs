using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using TechChallenge.Infrastructure.Database.Context;

namespace TechChallenge.Infrastructure.Database.Seeding
{
    public static class DataSeeder
    {
        public static async Task SeedAsync(IServiceProvider provider)
        {
            using var scope = provider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var configuration = scope.ServiceProvider.GetRequiredService<IConfiguration>();

            await AdminSeeder.SeedAsync(scope.ServiceProvider, context);

            if (bool.TryParse(configuration["Seed:FakeData"], out var seedFakeData) && seedFakeData)
                await FakeDataSeeder.SeedAsync(scope.ServiceProvider, context);

            if (bool.TryParse(configuration["Seed:DemoData"], out var seedDemoData) && seedDemoData)
                await DemoSeeder.SeedAsync(scope.ServiceProvider, context);
        }
    }
}
