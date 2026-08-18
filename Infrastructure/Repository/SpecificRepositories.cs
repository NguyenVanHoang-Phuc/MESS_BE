using Microsoft.EntityFrameworkCore;
using MESS.Domain.Entities;
using MESS.Domain.Interfaces;
using MESS.Infrastructure.Data;

namespace MESS.Infrastructure.Repository;

public class UserRepository : GenericRepository<User>, IUserRepository
{
    public UserRepository(MessDbContext context) : base(context) { }

    public async Task<User?> FindByUsernameAsync(string username)
        => await _dbSet
            .Include(u => u.Role)
            .Include(u => u.Department)
            .FirstOrDefaultAsync(u => u.Username == username);

    public async Task<IEnumerable<User>> GetAllWithDetailsAsync()
        => await _dbSet
            .Include(u => u.Role)
            .Include(u => u.Department)
            .AsNoTracking()
            .ToListAsync();

    public async Task<User?> GetByIdWithDetailsAsync(Guid id)
        => await _dbSet
            .Include(u => u.Role)
            .Include(u => u.Department)
            .FirstOrDefaultAsync(u => u.Id == id);
}

public class ConversationRepository : GenericRepository<Conversation>, IConversationRepository
{
    public ConversationRepository(MessDbContext context) : base(context) { }

    public async Task<IEnumerable<Conversation>> GetUserConversationsAsync(Guid userId)
        => await _dbSet
            .Include(c => c.Participants).ThenInclude(p => p.User)
            .Include(c => c.Messages.OrderByDescending(m => m.CreatedAt).Take(1)).ThenInclude(m => m.Sender)
            .Where(c => c.Participants.Any(p => p.UserId == userId))
            .OrderByDescending(c => c.Messages.Max(m => (DateTime?)m.CreatedAt) ?? c.CreatedAt)
            .AsNoTracking()
            .ToListAsync();

    public async Task<Conversation?> GetByIdWithDetailsAsync(Guid id)
        => await _dbSet
            .Include(c => c.Participants).ThenInclude(p => p.User)
            .FirstOrDefaultAsync(c => c.Id == id);

    public async Task<Conversation?> FindDirectConversationAsync(Guid userId1, Guid userId2)
        => await _dbSet
            .Where(c => c.Type == "Direct"
                && c.Participants.Any(p => p.UserId == userId1)
                && c.Participants.Any(p => p.UserId == userId2))
            .FirstOrDefaultAsync();

    public async Task<bool> IsParticipantAsync(Guid conversationId, Guid userId)
        => await _dbSet
            .AnyAsync(c => c.Id == conversationId && c.Participants.Any(p => p.UserId == userId));
}

public class MessageRepository : GenericRepository<Message>, IMessageRepository
{
    public MessageRepository(MessDbContext context) : base(context) { }

    public async Task<IEnumerable<Message>> GetConversationMessagesAsync(Guid conversationId, int pageNumber, int pageSize)
        => await _dbSet
            .Include(m => m.Sender)
            .Include(m => m.Attachments)
            .Include(m => m.MessageReactions).ThenInclude(r => r.User)
            .Where(m => m.ConversationId == conversationId && !m.IsDeleted)
            .OrderByDescending(m => m.CreatedAt)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .AsNoTracking()
            .ToListAsync();

    public async Task<int> GetConversationMessageCountAsync(Guid conversationId)
        => await _dbSet.CountAsync(m => m.ConversationId == conversationId && !m.IsDeleted);

    public async Task<Message?> GetByIdWithDetailsAsync(Guid id)
        => await _dbSet
            .Include(m => m.Sender)
            .Include(m => m.Attachments)
            .Include(m => m.MessageReactions).ThenInclude(r => r.User)
            .FirstOrDefaultAsync(m => m.Id == id);
}

public class ParticipantRepository : GenericRepository<Participant>, IParticipantRepository
{
    public ParticipantRepository(MessDbContext context) : base(context) { }

    public async Task<Participant?> FindAsync(Guid conversationId, Guid userId)
        => await _dbSet.FirstOrDefaultAsync(p => p.ConversationId == conversationId && p.UserId == userId);

    public async Task<IEnumerable<Participant>> GetConversationParticipantsAsync(Guid conversationId)
        => await _dbSet
            .Include(p => p.User)
            .Where(p => p.ConversationId == conversationId)
            .AsNoTracking()
            .ToListAsync();
}

public class TaskRepository : GenericRepository<MESS.Domain.Entities.Task>, ITaskRepository
{
    public TaskRepository(MessDbContext context) : base(context) { }

    public async Task<IEnumerable<MESS.Domain.Entities.Task>> GetAssignedToUserAsync(Guid userId)
        => await _dbSet
            .Include(t => t.Assignee)
            .Include(t => t.Creator)
            .Where(t => t.AssigneeId == userId)
            .OrderByDescending(t => t.CreatedAt)
            .AsNoTracking()
            .ToListAsync();

    public async Task<IEnumerable<MESS.Domain.Entities.Task>> GetCreatedByUserAsync(Guid userId)
        => await _dbSet
            .Include(t => t.Assignee)
            .Include(t => t.Creator)
            .Where(t => t.CreatedBy == userId)
            .OrderByDescending(t => t.CreatedAt)
            .AsNoTracking()
            .ToListAsync();

    public async Task<MESS.Domain.Entities.Task?> GetByIdWithDetailsAsync(Guid id)
        => await _dbSet
            .Include(t => t.Assignee)
            .Include(t => t.Creator)
            .Include(t => t.SourceMessage)
            .FirstOrDefaultAsync(t => t.Id == id);
}

public class MessageReactionRepository : GenericRepository<MessageReaction>, IMessageReactionRepository
{
    public MessageReactionRepository(MessDbContext context) : base(context) { }

    public async Task<MessageReaction?> FindAsync(Guid messageId, Guid userId, string emoji)
        => await _dbSet.FirstOrDefaultAsync(r =>
            r.MessageId == messageId && r.UserId == userId && r.EmojiCode == emoji);
}

public class DepartmentRepository : GenericRepository<Department>, IDepartmentRepository
{
    public DepartmentRepository(MessDbContext context) : base(context) { }

    public async Task<Department?> FindByNameAsync(string name)
        => await _dbSet.FirstOrDefaultAsync(d => d.Name == name);

    public async Task<IEnumerable<Department>> GetAllWithUsersAsync()
        => await _dbSet.Include(d => d.Users).AsNoTracking().ToListAsync();
}
