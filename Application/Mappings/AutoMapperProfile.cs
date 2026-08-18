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
                        UserNames = g.Select(r => r.User != null ? r.User.FullName : string.Empty).ToList()
                    }).ToList()));

        CreateMap<Message, MessageSummaryResponse>()
            .ForMember(d => d.SenderName, o => o.MapFrom(s => s.Sender != null ? s.Sender.FullName : string.Empty))
            .ForMember(d => d.SentAt, o => o.MapFrom(s => DateTime.SpecifyKind(s.CreatedAt, DateTimeKind.Utc)));

        CreateMap<Attachment, AttachmentResponse>()
            .ForMember(d => d.FileName, o => o.MapFrom(s => System.IO.Path.GetFileName(s.FileUrl)))
            .ForMember(d => d.FileType, o => o.MapFrom(s => s.FileType ?? string.Empty))
            .ForMember(d => d.FileSize, o => o.MapFrom(s => s.FileSize));

        // --- Task ---
        CreateMap<MESS.Domain.Entities.Task, TaskResponse>()
            .ForMember(d => d.AssigneeName, o => o.MapFrom(s => s.Assignee != null ? s.Assignee.FullName : null))
            .ForMember(d => d.CreatorName, o => o.MapFrom(s => s.Creator != null ? s.Creator.FullName : null))
            .ForMember(d => d.CreatorId, o => o.MapFrom(s => s.CreatedBy));
    }
}
