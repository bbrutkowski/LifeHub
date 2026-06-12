using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using System.Text.Json;

namespace LifeHub.Database
{
    public class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<ApplicationDbContext>
    {
        public ApplicationDbContext CreateDbContext(string[] args)
        {
            var current = Directory.GetCurrentDirectory();
            var apiAppsettings = FindApiAppSettings(current) ?? throw new InvalidOperationException("Nie znaleziono pliku appsettings.json dla LifeHub.Api.");

            var json = File.ReadAllText(apiAppsettings);
            using var doc = JsonDocument.Parse(json);

            if (!doc.RootElement.TryGetProperty("ConnectionStrings", out var connSection) ||
                !connSection.TryGetProperty("DefaultConnection", out var connValue) ||
                connValue.ValueKind != JsonValueKind.String)
            {
                throw new InvalidOperationException("Connection string 'DefaultConnection' nie została znaleziona w appsettings.json.");
            }

            var connectionString = connValue.GetString()!;
            var builder = new DbContextOptionsBuilder<ApplicationDbContext>();
            builder.UseSqlServer(connectionString);

            return new ApplicationDbContext(builder.Options);
        }

        private static string? FindApiAppSettings(string startDirectory)
        {
            var dir = new DirectoryInfo(startDirectory);
            while (dir is not null)
            {
                var mainPath = Path.Combine(dir.FullName, "LifeHub.Api", "appsettings.json");
                if (File.Exists(mainPath)) return mainPath;

                var alternativePath = Path.Combine(dir.FullName, "appsettings.json");
                if (File.Exists(alternativePath)) return alternativePath;

                dir = dir.Parent;
            }

            return null;
        }
    }
}
