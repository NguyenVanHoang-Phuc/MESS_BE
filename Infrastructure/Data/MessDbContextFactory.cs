using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace MESS.Infrastructure.Data;

/// <summary>
/// Factory dùng cho EF Core Design-time (migrations) khi chạy độc lập từ Infrastructure project.
/// </summary>
public class MessDbContextFactory : IDesignTimeDbContextFactory<MessDbContext>
{
    public MessDbContext CreateDbContext(string[] args)
    {
        // Đọc appsettings.json từ thư mục hiện tại (Infrastructure)
        var configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: true)
            .Build();

        var connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? "Server=localhost;Database=MessDb;Trusted_Connection=True;TrustServerCertificate=True;";

        var optionsBuilder = new DbContextOptionsBuilder<MessDbContext>();
        optionsBuilder.UseSqlServer(connectionString);

        return new MessDbContext(optionsBuilder.Options);
    }
}
