using MESS.Application.Interfaces.Auth;
using MESS.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace MESS.Infrastructure.Data;

public class DatabaseSeeder
{
    private readonly MessDbContext _context;
    private readonly IPasswordHasher _passwordHasher;
    private readonly ILogger<DatabaseSeeder> _logger;

    public DatabaseSeeder(MessDbContext context, IPasswordHasher passwordHasher, ILogger<DatabaseSeeder> logger)
    {
        _context = context;
        _passwordHasher = passwordHasher;
        _logger = logger;
    }

    public async Task<string> SeedAsync()
    {
        try
        {
            // Tự động Apply Migration nếu có
            if (_context.Database.IsSqlServer())
            {
                await _context.Database.MigrateAsync();
            }

            if (!await _context.Set<User>().AnyAsync(u => u.Username == "userA"))
            {
                _logger.LogInformation("Bắt đầu Seed Data...");

                // 1. Tạo 2 User
                var userA = new User
                {
                    Id = Guid.NewGuid(),
                    Username = "userA",
                    FullName = "Nguyễn Văn A",
                    PasswordHash = _passwordHasher.Hash("123456"),
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                };

                var userB = new User
                {
                    Id = Guid.NewGuid(),
                    Username = "userB",
                    FullName = "Trần Thị B",
                    PasswordHash = _passwordHasher.Hash("123456"),
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                };

                await _context.Set<User>().AddRangeAsync(userA, userB);

                // 2. Tạo 1 Conversation Direct giữa 2 người
                var conversation = new Conversation
                {
                    Id = Guid.NewGuid(),
                    Title = "Cuộc trò chuyện giữa A và B",
                    Type = "Direct",
                    CreatedBy = userA.Id,
                    CreatedAt = DateTime.UtcNow
                };
                
                await _context.Set<Conversation>().AddAsync(conversation);

                // 3. Thêm Participants
                var participantA = new Participant
                {
                    ConversationId = conversation.Id,
                    UserId = userA.Id,
                    Role = "Member",
                    JoinedAt = DateTime.UtcNow
                };

                var participantB = new Participant
                {
                    ConversationId = conversation.Id,
                    UserId = userB.Id,
                    Role = "Member",
                    JoinedAt = DateTime.UtcNow
                };

                await _context.Set<Participant>().AddRangeAsync(participantA, participantB);
                
                await _context.SaveChangesAsync();
                
                _logger.LogInformation("Seed Data thành công!");
                _logger.LogInformation($"[User A] Username: userA | Pass: 123456 | Id: {userA.Id}");
                _logger.LogInformation($"[User B] Username: userB | Pass: 123456 | Id: {userB.Id}");
                _logger.LogInformation($"[Conversation ID]: {conversation.Id}");
                
                return $"Seed data thành công! Đã tạo userA và userB. Conversation ID: {conversation.Id}";
            }
            
            _logger.LogInformation("Data đã tồn tại, bỏ qua Seed.");
            return "Data (userA) đã tồn tại trong Database, không cần Seed thêm.";
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Có lỗi xảy ra trong quá trình Seed Data!");
            throw;
        }
    }
}
