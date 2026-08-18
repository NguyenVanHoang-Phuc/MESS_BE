using AutoMapper;
using MediatR;
using MESS.Application.DTOs.Responses.Conversations;
using MESS.Domain.Errors;
using MESS.Domain.Interfaces;
using MESS.Domain.Shared;

namespace MESS.Application.UseCases.Conversations.Commands.RemoveParticipant;

public class RemoveParticipantCommandHandler : IRequestHandler<RemoveParticipantCommand, Result<ConversationResponse>>
{
    private readonly IConversationRepository _conversationRepository;
    private readonly IParticipantRepository _participantRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public RemoveParticipantCommandHandler(
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
        RemoveParticipantCommand request, CancellationToken cancellationToken)
    {
        var conversation = await _conversationRepository.GetByIdWithDetailsAsync(request.ConversationId);
        if (conversation is null)
            return Result<ConversationResponse>.Failure(DomainErrors.Conversation.NotFoundById(request.ConversationId));

        if (conversation.Type != "Group")
            return Result<ConversationResponse>.Failure(DomainErrors.Conversation.NotGroup);

        // Kiểm tra quyền: Người yêu cầu phải là Admin của nhóm
        var requesterParticipant = await _participantRepository.FindAsync(request.ConversationId, request.RequesterId);
        if (requesterParticipant is null || requesterParticipant.Role != "Admin")
            return Result<ConversationResponse>.Failure(DomainErrors.Conversation.NotAdmin);

        // Không được tự xóa Admin/Creator
        if (request.UserIdToRemove == conversation.CreatedBy)
            return Result<ConversationResponse>.Failure(DomainErrors.Conversation.CannotRemoveAdmin);

        // Tìm thành viên cần xóa
        var targetParticipant = await _participantRepository.FindAsync(request.ConversationId, request.UserIdToRemove);
        if (targetParticipant is null)
            return Result<ConversationResponse>.Failure(DomainErrors.Conversation.ParticipantNotFound);

        // Xóa thành viên
        _participantRepository.Remove(targetParticipant);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var updatedConversation = await _conversationRepository.GetByIdWithDetailsAsync(request.ConversationId);
        var response = _mapper.Map<ConversationResponse>(updatedConversation);
        return Result<ConversationResponse>.Success(response);
    }
}
