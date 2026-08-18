using AutoMapper;
using MediatR;
using MESS.Application.DTOs.Responses.Conversations;
using MESS.Domain.Entities;
using MESS.Domain.Errors;
using MESS.Domain.Interfaces;
using MESS.Domain.Shared;

namespace MESS.Application.UseCases.Conversations.Commands.CreateConversation;

public class CreateConversationCommandHandler : IRequestHandler<CreateConversationCommand, Result<ConversationResponse>>
{
    private readonly IConversationRepository _conversationRepository;
    private readonly IParticipantRepository _participantRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public CreateConversationCommandHandler(
        IConversationRepository conversationRepository,
        IParticipantRepository participantRepository,
        IUnitOfWork unitOfWork,
        IMapper mapper)
    {
        _conversationRepository = conversationRepository;
        _participantRepository = participantRepository;
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<Result<ConversationResponse>> Handle(
        CreateConversationCommand request, CancellationToken cancellationToken)
    {
        // Validate: Direct conversation không được tạo với chính mình
        if (request.Type == "Direct")
        {
            if (request.ParticipantIds.Count != 1)
                return Result<ConversationResponse>.Failure(
                    new Error("Conversation.InvalidParticipants", "Direct conversation must have exactly one other participant."));

            var targetUserId = request.ParticipantIds[0];
            if (targetUserId == request.CreatorId)
                return Result<ConversationResponse>.Failure(DomainErrors.Conversation.CannotCreateDirectWithSelf);

            // Kiểm tra nếu đã tồn tại conversation direct giữa 2 người này
            var existing = await _conversationRepository.FindDirectConversationAsync(request.CreatorId, targetUserId);
            if (existing is not null)
                return Result<ConversationResponse>.Failure(DomainErrors.Conversation.DirectConversationAlreadyExists);
        }

        // Tạo conversation
        var conversation = new Conversation
        {
            Id = Guid.NewGuid(),
            Type = request.Type,
            Title = request.Title,
            CreatedBy = request.CreatorId,
            CreatedAt = DateTime.UtcNow
        };
        await _conversationRepository.AddAsync(conversation);

        // Thêm creator làm Admin participant
        var creatorParticipant = new Participant
        {
            ConversationId = conversation.Id,
            UserId = request.CreatorId,
            Role = "Admin",
            JoinedAt = DateTime.UtcNow
        };
        await _participantRepository.AddAsync(creatorParticipant);

        // Thêm các participant còn lại
        foreach (var participantId in request.ParticipantIds)
        {
            var participant = new Participant
            {
                ConversationId = conversation.Id,
                UserId = participantId,
                Role = "Member",
                JoinedAt = DateTime.UtcNow
            };
            await _participantRepository.AddAsync(participant);
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var fullConversation = await _conversationRepository.GetByIdWithDetailsAsync(conversation.Id);
        var response = _mapper.Map<ConversationResponse>(fullConversation);
        return Result<ConversationResponse>.Success(response);
    }
}
