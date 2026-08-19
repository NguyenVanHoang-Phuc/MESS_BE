using MESS.Domain.Entities;

namespace MESS.Domain.Interfaces;

public interface IUserRepository : IGenericRepository<User>
{
    Task<User?> FindByUsernameAsync(string username);
    Task<IEnumerable<User>> GetAllWithDetailsAsync();
    Task<User?> GetByIdWithDetailsAsync(Guid id);
    Task<List<User>> SearchUsersAsync(string query, Guid currentUserId, int limit);
}

public interface IConversationRepository : IGenericRepository<Conversation>
{
    Task<IEnumerable<Conversation>> GetUserConversationsAsync(Guid userId);
    Task<Conversation?> GetByIdWithDetailsAsync(Guid id);
    Task<Conversation?> FindDirectConversationAsync(Guid userId1, Guid userId2);
    Task<Conversation?> FindByCanonicalKeyAsync(string canonicalKey);
    Task<bool> IsParticipantAsync(Guid conversationId, Guid userId);
}

public interface IMessageRepository : IGenericRepository<Message>
{
    Task<IEnumerable<Message>> GetConversationMessagesAsync(Guid conversationId, int pageNumber, int pageSize);
    Task<List<Message>> GetConversationMessagesByCursorAsync(Guid conversationId, DateTime? beforeCursor, int limit);
    Task<int> GetConversationMessageCountAsync(Guid conversationId);
    Task<Message?> GetByIdWithDetailsAsync(Guid id);
    Task<List<Message>> GetUnreadMessagesAsync(Guid conversationId, Guid readerId);
    Task<Dictionary<Guid, int>> GetUnreadCountsAsync(List<Guid> conversationIds, Guid userId);
    System.Threading.Tasks.Task AddMessageReadsAsync(IEnumerable<MessageRead> messageReads);
    Task<(List<Message> Items, int TotalCount)> SearchMessagesAsync(
        Guid currentUserId,
        string? keyword,
        Guid? senderId,
        Guid? conversationId,
        DateTime? fromDate,
        DateTime? toDate,
        bool? hasAttachments,
        string? fileType,
        int pageNumber,
        int pageSize);
}

public interface IParticipantRepository : IGenericRepository<Participant>
{
    Task<Participant?> FindAsync(Guid conversationId, Guid userId);
    Task<IEnumerable<Participant>> GetConversationParticipantsAsync(Guid conversationId);
}

public interface ITaskRepository : IGenericRepository<MESS.Domain.Entities.Task>
{
    Task<IEnumerable<MESS.Domain.Entities.Task>> GetAssignedToUserAsync(Guid userId);
    Task<IEnumerable<MESS.Domain.Entities.Task>> GetCreatedByUserAsync(Guid userId);
    Task<MESS.Domain.Entities.Task?> GetByIdWithDetailsAsync(Guid id);
    Task<List<MESS.Domain.Entities.Task>> GetTasksByFilterAsync(Guid? conversationId, Guid? messageId, Guid? assigneeId, Guid? creatorId, string? status);
}

public interface IMessageReactionRepository : IGenericRepository<MessageReaction>
{
    Task<MessageReaction?> FindAsync(Guid messageId, Guid userId, string emoji);
    Task<MessageReaction?> FindByUserAsync(Guid messageId, Guid userId);
}

public interface IDepartmentRepository : IGenericRepository<Department>
{
    Task<Department?> FindByNameAsync(string name);
    Task<IEnumerable<Department>> GetAllWithUsersAsync();
}
