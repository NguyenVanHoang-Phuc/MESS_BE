using AutoMapper;
using MediatR;
using MESS.Application.DTOs.Responses.Conversations;
using MESS.Domain.Entities;
using MESS.Domain.Errors;
using MESS.Domain.Interfaces;
using MESS.Domain.Shared;

namespace MESS.Application.UseCases.Conversations.Commands.AddParticipant;

public class AddParticipantCommandHandler : IRequestHandler<AddParticipantCommand, Result<ConversationResponse>>
{
    private readonly IConversationRepository _conversationRepository;
    private readonly IParticipantRepository _participantRepository;
    private readonly IUserRepository _userRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public AddParticipantCommandHandler(
        IConversationRepository conversationRepository,
        IParticipantRepository participantRepository,
        IUserRepository userRepository,
        IUnitOfWork unitOfWork,
        IMapper mapper)
    {
        _conversationRepository = conversationRepository;
        _participantRepository = participantRepository;
        _userRepository = userRepository;
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<Result<ConversationResponse>> Handle(
        AddParticipantCommand request, CancellationToken cancellationToken)
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

        // Kiểm tra user cần thêm có tồn tại không
        var userToAdd = await _userRepository.GetByIdWithDetailsAsync(request.UserIdToAdd);
        if (userToAdd is null)
            return Result<ConversationResponse>.Failure(DomainErrors.User.NotFoundById(request.UserIdToAdd));

        // Kiểm tra xem đã là thành viên chưa
        var existingParticipant = await _participantRepository.FindAsync(request.ConversationId, request.UserIdToAdd);
        if (existingParticipant is not null)
            return Result<ConversationResponse>.Failure(DomainErrors.Conversation.ParticipantAlreadyExists);

        // Thêm thành viên mới
        var newParticipant = new Participant
        {
            ConversationId = request.ConversationId,
            UserId = request.UserIdToAdd,
            Role = request.Role,
            JoinedAt = DateTime.UtcNow
        };

        await _participantRepository.AddAsync(newParticipant);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var updatedConversation = await _conversationRepository.GetByIdWithDetailsAsync(request.ConversationId);
        var response = _mapper.Map<ConversationResponse>(updatedConversation);
        return Result<ConversationResponse>.Success(response);
    }
}
