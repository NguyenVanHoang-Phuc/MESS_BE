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

    public async Task<List<User>> SearchUsersAsync(string query, Guid currentUserId, int limit)
    {
        var q = _dbSet
            .Include(u => u.Role)
            .Include(u => u.Department)
            .Where(u => u.IsActive && u.Id != currentUserId);

        var trimmed = query?.Trim() ?? string.Empty;
        if (!string.IsNullOrWhiteSpace(trimmed))
        {
            q = q.Where(u => u.FullName.Contains(trimmed) ||
                             u.Username.Contains(trimmed) ||
                             (u.Department != null && u.Department.Name.Contains(trimmed)));
        }

        return await q
            .OrderBy(u => u.FullName)
            .Take(limit > 0 ? limit : 20)
            .AsNoTracking()
            .ToListAsync();
    }
}

public class ConversationRepository : GenericRepository<Conversation>, IConversationRepository
{
    public ConversationRepository(MessDbContext context) : base(context) { }

    public async Task<IEnumerable<Conversation>> GetUserConversationsAsync(Guid userId)
        => await _dbSet
            .Include(c => c.Participants).ThenInclude(p => p.User)
            .Include(c => c.Messages.OrderByDescending(m => m.CreatedAt).Take(1)).ThenInclude(m => m.Sender)
            .Include(c => c.Messages.OrderByDescending(m => m.CreatedAt).Take(1)).ThenInclude(m => m.Attachments)
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

    public async Task<Conversation?> FindByCanonicalKeyAsync(string canonicalKey)
        => await _dbSet
            .Include(c => c.Participants).ThenInclude(p => p.User)
            .FirstOrDefaultAsync(c => c.CanonicalKey == canonicalKey);

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
            .Include(m => m.MessageReads).ThenInclude(r => r.User)
            .Where(m => m.ConversationId == conversationId && !m.IsDeleted)
            .OrderByDescending(m => m.CreatedAt)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .AsNoTracking()
            .ToListAsync();

    public async Task<List<Message>> GetConversationMessagesByCursorAsync(Guid conversationId, DateTime? beforeCursor, int limit)
    {
        var query = _dbSet
            .Include(m => m.Sender)
            .Include(m => m.Attachments)
            .Include(m => m.MessageReactions).ThenInclude(r => r.User)
            .Include(m => m.MessageReads).ThenInclude(r => r.User)
            .Where(m => m.ConversationId == conversationId && !m.IsDeleted);

        if (beforeCursor.HasValue)
        {
            query = query.Where(m => m.CreatedAt < beforeCursor.Value);
        }

        return await query
            .OrderByDescending(m => m.CreatedAt)
            .Take(limit)
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task<int> GetConversationMessageCountAsync(Guid conversationId)
        => await _dbSet.CountAsync(m => m.ConversationId == conversationId && !m.IsDeleted);

    public async Task<Message?> GetByIdWithDetailsAsync(Guid id)
        => await _dbSet
            .Include(m => m.Sender)
            .Include(m => m.Attachments)
            .Include(m => m.MessageReactions).ThenInclude(r => r.User)
            .Include(m => m.MessageReads).ThenInclude(r => r.User)
            .FirstOrDefaultAsync(m => m.Id == id);

    public async Task<List<Message>> GetUnreadMessagesAsync(Guid conversationId, Guid readerId)
        => await _dbSet
            .Where(m => m.ConversationId == conversationId &&
                        m.SenderId != readerId &&
                        !m.IsDeleted &&
                        !m.MessageReads.Any(mr => mr.UserId == readerId))
            .ToListAsync();

    public async Task<Dictionary<Guid, int>> GetUnreadCountsAsync(List<Guid> conversationIds, Guid userId)
    {
        return await _dbSet
            .Where(m => conversationIds.Contains(m.ConversationId) &&
                        m.SenderId != userId &&
                        !m.IsDeleted &&
                        !m.MessageReads.Any(mr => mr.UserId == userId))
            .GroupBy(m => m.ConversationId)
            .Select(g => new { ConversationId = g.Key, Count = g.Count() })
            .ToDictionaryAsync(x => x.ConversationId, x => x.Count);
    }

    public async System.Threading.Tasks.Task AddMessageReadsAsync(IEnumerable<MessageRead> messageReads)
    {
        await _context.Set<MessageRead>().AddRangeAsync(messageReads);
    }

    public async Task<(List<Message> Items, int TotalCount)> SearchMessagesAsync(
        Guid currentUserId,
        string? keyword,
        Guid? senderId,
        Guid? conversationId,
        DateTime? fromDate,
        DateTime? toDate,
        bool? hasAttachments,
        string? fileType,
        int pageNumber,
        int pageSize)
    {
        var query = _dbSet
            .Include(m => m.Sender)
            .Include(m => m.Conversation).ThenInclude(c => c.Participants).ThenInclude(p => p.User)
            .Include(m => m.Attachments)
            .Where(m => !m.IsDeleted && m.Conversation.Participants.Any(p => p.UserId == currentUserId));

        if (conversationId.HasValue && conversationId.Value != Guid.Empty)
        {
            query = query.Where(m => m.ConversationId == conversationId.Value);
        }

        if (senderId.HasValue && senderId.Value != Guid.Empty)
        {
            query = query.Where(m => m.SenderId == senderId.Value);
        }

        if (!string.IsNullOrWhiteSpace(keyword))
        {
            var kw = keyword.Trim();
            query = query.Where(m => m.Content != null && m.Content.Contains(kw));
        }

        if (fromDate.HasValue)
        {
            query = query.Where(m => m.CreatedAt >= fromDate.Value);
        }

        if (toDate.HasValue)
        {
            query = query.Where(m => m.CreatedAt <= toDate.Value);
        }

        if (hasAttachments.HasValue && hasAttachments.Value)
        {
            query = query.Where(m => m.Attachments.Any());
        }

        if (!string.IsNullOrWhiteSpace(fileType))
        {
            var ft = fileType.Trim().ToLower();
            query = query.Where(m => m.Attachments.Any(a => a.FileType != null && a.FileType.ToLower().Contains(ft)));
        }

        var totalCount = await query.CountAsync();

        var page = pageNumber > 0 ? pageNumber : 1;
        var size = pageSize > 0 ? pageSize : 20;

        var items = await query
            .OrderByDescending(m => m.CreatedAt)
            .Skip((page - 1) * size)
            .Take(size)
            .AsNoTracking()
            .ToListAsync();

        return (items, totalCount);
    }
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

    public async Task<MessageReaction?> FindByUserAsync(Guid messageId, Guid userId)
        => await _dbSet.FirstOrDefaultAsync(r =>
            r.MessageId == messageId && r.UserId == userId);
}

public class DepartmentRepository : GenericRepository<Department>, IDepartmentRepository
{
    public DepartmentRepository(MessDbContext context) : base(context) { }

    public async Task<Department?> FindByNameAsync(string name)
        => await _dbSet.FirstOrDefaultAsync(d => d.Name == name);

    public async Task<IEnumerable<Department>> GetAllWithUsersAsync()
        => await _dbSet.Include(d => d.Users).AsNoTracking().ToListAsync();
}
