using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace TechChallenge.Infrastructure.Database.Context
{
    public class ApplicationDbContextFactory : IDesignTimeDbContextFactory<ApplicationDbContext>
    {
        private const string ApiProjectName = "TechChallenge.Api";

        public ApplicationDbContext CreateDbContext(string[] args)
        {
            var currentDir = Directory.GetCurrentDirectory();

            // Try several candidate locations to find the API project folder where appsettings.json is expected
            var candidates = new[]
            {
                Path.Combine(currentDir, "..", "..", ApiProjectName),
                Path.Combine(currentDir, "..", "..", "..", ApiProjectName),
                Path.Combine(currentDir, "..", ApiProjectName),
                Path.Combine(currentDir, ApiProjectName),
                currentDir
            };

            string? apiPath = null;
            foreach (var c in candidates)
            {
                var full = Path.GetFullPath(c);
                if (Directory.Exists(full))
                {
                    apiPath = full;
                    break;
                }
            }

            apiPath ??= currentDir;

            var config = new ConfigurationBuilder()
                .SetBasePath(apiPath)
                .AddJsonFile("appsettings.json", optional: true)
                .AddEnvironmentVariables()
                .Build();

            var connectionString = config.GetConnectionString("DefaultConnection")
                                   ?? Environment.GetEnvironmentVariable("DefaultConnection");

            var optionsBuilder = new DbContextOptionsBuilder<ApplicationDbContext>();

            if (string.IsNullOrWhiteSpace(connectionString))
            {
                throw new InvalidOperationException($"Connection string 'DefaultConnection' not found. Searched base path: {apiPath}");
            }

            optionsBuilder.UseNpgsql(connectionString, b => b.MigrationsAssembly(typeof(ApplicationDbContext).Assembly.FullName));

            return new ApplicationDbContext(optionsBuilder.Options);
        }
    }
}
