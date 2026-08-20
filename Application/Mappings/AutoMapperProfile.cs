using AutoMapper;
using MESS.Domain.Entities;
using MESS.Application.DTOs.Responses.Auth;
using MESS.Application.DTOs.Responses.Users;
using MESS.Application.DTOs.Responses.Conversations;
using MESS.Application.DTOs.Responses.Messages;
using MESS.Application.DTOs.Responses.Tasks;

namespace MESS.Application.Mappings;

public class AutoMapperProfile : Profile
{
    public AutoMapperProfile()
    {
        // --- User ---
        CreateMap<User, UserResponse>()
            .ForMember(d => d.DepartmentName, o => o.MapFrom(s => s.Department != null ? s.Department.Name : null))
            .ForMember(d => d.RoleName, o => o.MapFrom(s => s.Role != null ? s.Role.Name : null));

        CreateMap<User, LoginResponse>()
            .ForMember(d => d.UserId, o => o.MapFrom(s => s.Id))
            .ForMember(d => d.RoleName, o => o.MapFrom(s => s.Role != null ? s.Role.Name : null))
            .ForMember(d => d.DepartmentName, o => o.MapFrom(s => s.Department != null ? s.Department.Name : null))
            .ForMember(d => d.AccessToken, o => o.Ignore()); // set manually in handler

        // --- Conversation ---
        CreateMap<Conversation, ConversationResponse>()
            .ForMember(d => d.LastMessage, o => o.MapFrom(s =>
                s.Messages.OrderByDescending(m => m.CreatedAt).FirstOrDefault()));

        CreateMap<Participant, ParticipantResponse>()
            .ForMember(d => d.Username, o => o.MapFrom(s => s.User != null ? s.User.Username : string.Empty))
            .ForMember(d => d.FullName, o => o.MapFrom(s => s.User != null ? s.User.FullName : string.Empty));

        // --- Message ---
        CreateMap<Message, MessageResponse>()
            .ForMember(d => d.SenderName, o => o.MapFrom(s => s.Sender != null ? s.Sender.FullName : string.Empty))
            .ForMember(d => d.SentAt, o => o.MapFrom(s => DateTime.SpecifyKind(s.CreatedAt, DateTimeKind.Utc)))
            .ForMember(d => d.Reactions, o => o.MapFrom(s =>
                s.MessageReactions
                    .GroupBy(r => r.EmojiCode)
                    .Select(g => new ReactionResponse
                    {
                        Emoji = g.Key,
                        Count = g.Count(),
                        UserNames = g.Select(r => r.User != null ? r.User.FullName : string.Empty).ToList(),
                        UserIds = g.Select(r => r.UserId).ToList()
                    }).ToList()))
            .ForMember(d => d.Reads, o => o.MapFrom(s => s.MessageReads));

        CreateMap<MessageRead, MessageReadResponse>()
            .ForMember(d => d.UserId, o => o.MapFrom(s => s.UserId))
            .ForMember(d => d.UserName, o => o.MapFrom(s => s.User != null ? s.User.Username : string.Empty))
            .ForMember(d => d.FullName, o => o.MapFrom(s => s.User != null ? s.User.FullName : string.Empty))
            .ForMember(d => d.ReadAt, o => o.MapFrom(s => DateTime.SpecifyKind(s.ReadAt, DateTimeKind.Utc)));

        CreateMap<Message, MessageSummaryResponse>()
            .ForMember(d => d.SenderName, o => o.MapFrom(s => s.Sender != null ? s.Sender.FullName : string.Empty))
            .ForMember(d => d.SentAt, o => o.MapFrom(s => DateTime.SpecifyKind(s.CreatedAt, DateTimeKind.Utc)))
            .ForMember(d => d.Content, o => o.MapFrom(s =>
                !string.IsNullOrEmpty(s.Content)
                    ? s.Content
                    : s.Attachments != null && s.Attachments.Any(a =>
                        (!string.IsNullOrEmpty(a.FileType) && a.FileType.StartsWith("image/")) ||
                        a.FileUrl.Contains("image/upload") ||
                        a.FileUrl.EndsWith(".jpg") || a.FileUrl.EndsWith(".jpeg") || a.FileUrl.EndsWith(".png") || a.FileUrl.EndsWith(".webp") || a.FileUrl.EndsWith(".gif"))
                        ? (s.Attachments.Count > 1 ? $"[Đã gửi {s.Attachments.Count} hình ảnh]" : "[Hình ảnh]")
                        : s.Attachments != null && s.Attachments.Any(a =>
                            (!string.IsNullOrEmpty(a.FileType) && a.FileType.StartsWith("video/")) ||
                            a.FileUrl.Contains("video/upload") ||
                            a.FileUrl.EndsWith(".mp4") || a.FileUrl.EndsWith(".mov"))
                            ? "[Video]"
                            : s.Attachments != null && s.Attachments.Any()
                                ? (s.Attachments.Count > 1 ? $"[Đã gửi {s.Attachments.Count} tệp đính kèm]" : $"[Tệp] {System.IO.Path.GetFileName(s.Attachments.First().FileUrl)}")
                                : "[Hình ảnh/Tệp]"));

        CreateMap<Attachment, AttachmentResponse>()
            .ForMember(d => d.FileName, o => o.MapFrom(s => ExtractCleanFileName(s.FileUrl)))
            .ForMember(d => d.FileType, o => o.MapFrom(s => s.FileType ?? string.Empty))
            .ForMember(d => d.FileSize, o => o.MapFrom(s => s.FileSize));

        // --- Task ---
        CreateMap<MESS.Domain.Entities.Task, TaskResponse>()
            .ForMember(d => d.Description, o => o.MapFrom(s => ExtractCleanDescription(s.Description)))
            .ForMember(d => d.AssigneeName, o => o.MapFrom(s => s.Assignee != null ? s.Assignee.FullName : null))
            .ForMember(d => d.CreatorName, o => o.MapFrom(s => s.Creator != null ? s.Creator.FullName : null))
            .ForMember(d => d.CreatorId, o => o.MapFrom(s => s.CreatedBy))
            .ForMember(d => d.ConversationId, o => o.MapFrom(s => s.SourceMessage != null ? s.SourceMessage.ConversationId : ParseGuidOrNull(s.RefId)))
            .ForMember(d => d.Priority, o => o.MapFrom(s => ExtractPriority(s.RefType)))
            .ForMember(d => d.CreatedAt, o => o.MapFrom(s => DateTime.SpecifyKind(s.CreatedAt, DateTimeKind.Utc)))
            .ForMember(d => d.Deadline, o => o.MapFrom(s => s.Deadline.HasValue ? DateTime.SpecifyKind(s.Deadline.Value, DateTimeKind.Utc) : (DateTime?)null));
    }

    private static string ExtractCleanFileName(string? url)
    {
        if (string.IsNullOrWhiteSpace(url)) return "attachment";
        try
        {
            var clean = url.Split('?')[0].Split('#')[0];
            clean = System.IO.Path.GetFileName(clean);
            if (clean.Length > 33 && clean[32] == '_' && System.Text.RegularExpressions.Regex.IsMatch(clean.Substring(0, 32), "^[a-fA-F0-9]{32}$"))
            {
                return clean.Substring(33);
            }
            return clean;
        }
        catch
        {
            return "attachment";
        }
    }

    private static string? ExtractCleanDescription(string? desc)
    {
        if (string.IsNullOrEmpty(desc)) return desc;
        var clean = System.Text.RegularExpressions.Regex.Replace(desc, @"<!--ASSIGNEES:.*?-->", "");
        clean = System.Text.RegularExpressions.Regex.Replace(clean, @"<!--REMINDED:.*?-->", "");
        return clean.TrimEnd();
    }

    private static Guid? ParseGuidOrNull(string? str)
    {
        if (string.IsNullOrEmpty(str)) return null;
        return Guid.TryParse(str, out var g) ? g : null;
    }

    private static string ExtractPriority(string? refType)
    {
        if (string.IsNullOrEmpty(refType)) return "Medium";
        var idx = refType.IndexOf('#');
        return idx >= 0 ? refType.Substring(0, idx) : refType;
    }
}
