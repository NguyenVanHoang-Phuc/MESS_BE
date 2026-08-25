using MESS.Application.Interfaces.Auth;
using MESS.Domain.Interfaces;
using MESS.Infrastructure.Data;
using MESS.Infrastructure.Repository;
using MESS.Infrastructure.Services.Auth;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace MESS.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // Database
        services.AddDbContext<MessDbContext>(options =>
            options.UseSqlServer(configuration.GetConnectionString("DefaultConnection")));

        // Unit of Work
        services.AddScoped<IUnitOfWork, UnitOfWork>();
        
        // Seeder
        services.AddScoped<DatabaseSeeder>();

        // Generic Repository
        services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));

        // Specific Repositories
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IConversationRepository, ConversationRepository>();
        services.AddScoped<IMessageRepository, MessageRepository>();
        services.AddScoped<IParticipantRepository, ParticipantRepository>();
        services.AddScoped<ITaskRepository, TaskRepository>();
        services.AddScoped<IMessageReactionRepository, MessageReactionRepository>();
        services.AddScoped<IDepartmentRepository, DepartmentRepository>();
        services.AddScoped<IRoleRepository, RoleRepository>();

        // Auth Services
        services.AddScoped<ITokenService, TokenService>();
        services.AddScoped<IPasswordHasher, PasswordHasher>();
        services.AddScoped<ICurrentUser, CurrentUser>();

        // Org Sync Service (MES-015)
        services.AddScoped<MESS.Application.Interfaces.Org.IOrgSyncService, MESS.Infrastructure.Services.OrgSyncService>();

        // Zalo OA / ZNS Notification Simulator Service
        services.AddScoped<MESS.Application.Interfaces.Zalo.IZaloNotificationService, MESS.Infrastructure.Services.Zalo.ZaloNotificationSimulatorService>();

        // HttpContext (needed for CurrentUser)
        services.AddHttpContextAccessor();

        return services;
    }
}
