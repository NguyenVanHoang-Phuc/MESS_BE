using System;
using System.IO;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace MESS.Infrastructure.Data;

/// <summary>
/// Factory dùng cho EF Core Design-time (migrations) khi chạy độc lập từ Infrastructure project hoặc CLI.
/// </summary>
public class MessDbContextFactory : IDesignTimeDbContextFactory<MessDbContext>
{
    public MessDbContext CreateDbContext(string[] args)
    {
        var currentDir = Directory.GetCurrentDirectory();

        // 1. Đọc file .env nếu có
        LoadEnvIfExists(Path.Combine(currentDir, ".env"));
        LoadEnvIfExists(Path.Combine(currentDir, "Mess", ".env"));
        LoadEnvIfExists(Path.Combine(currentDir, "..", "Mess", ".env"));

        // 2. Build Configuration từ appsettings.json và Environment Variables
        var builder = new ConfigurationBuilder()
            .SetBasePath(currentDir)
            .AddJsonFile("appsettings.json", optional: true)
            .AddJsonFile(Path.Combine(currentDir, "Mess", "appsettings.json"), optional: true)
            .AddJsonFile(Path.Combine(currentDir, "..", "Mess", "appsettings.json"), optional: true)
            .AddEnvironmentVariables();

        var configuration = builder.Build();

        var connectionString = Environment.GetEnvironmentVariable("ConnectionStrings__DefaultConnection")
            ?? configuration.GetConnectionString("DefaultConnection")
            ?? "Server=localhost;Database=MessDb;Trusted_Connection=True;TrustServerCertificate=True;";

        var optionsBuilder = new DbContextOptionsBuilder<MessDbContext>();
        optionsBuilder.UseSqlServer(connectionString);

        return new MessDbContext(optionsBuilder.Options);
    }

    private static void LoadEnvIfExists(string filePath)
    {
        try
        {
            if (!File.Exists(filePath)) return;

            foreach (var line in File.ReadAllLines(filePath))
            {
                var trimmed = line.Trim();
                if (string.IsNullOrWhiteSpace(trimmed) || trimmed.StartsWith('#')) continue;

                var separatorIndex = trimmed.IndexOf('=');
                if (separatorIndex <= 0) continue;

                var key = trimmed.Substring(0, separatorIndex).Trim();
                var value = trimmed.Substring(separatorIndex + 1).Trim().Trim('"');

                if (!string.IsNullOrEmpty(key) && string.IsNullOrEmpty(Environment.GetEnvironmentVariable(key)))
                {
                    Environment.SetEnvironmentVariable(key, value);
                }
            }
        }
        catch
        {
            // Ignore parse errors
        }
    }
}
