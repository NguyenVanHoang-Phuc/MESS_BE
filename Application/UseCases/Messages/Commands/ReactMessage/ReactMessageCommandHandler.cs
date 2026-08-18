using AutoMapper;
using MediatR;
using MESS.Application.DTOs.Responses.Messages;
using MESS.Application.Interfaces.Notifications;
using MESS.Domain.Entities;
using MESS.Domain.Errors;
using MESS.Domain.Interfaces;
using MESS.Domain.Shared;

namespace MESS.Application.UseCases.Messages.Commands.ReactMessage;

public class ReactMessageCommandHandler : IRequestHandler<ReactMessageCommand, Result<List<ReactionResponse>>>
{
    private readonly IMessageRepository _messageRepository;
    private readonly IConversationRepository _conversationRepository;
    private readonly IMessageReactionRepository _reactionRepository;
    private readonly IParticipantRepository _participantRepository;
    private readonly IChatNotificationService _chatNotificationService;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public ReactMessageCommandHandler(
        IMessageRepository messageRepository,
        IConversationRepository conversationRepository,
        IMessageReactionRepository reactionRepository,
        IParticipantRepository participantRepository,
        IChatNotificationService chatNotificationService,
        IUnitOfWork unitOfWork,
        IMapper mapper)
    {
        _messageRepository = messageRepository;
        _conversationRepository = conversationRepository;
        _reactionRepository = reactionRepository;
        _participantRepository = participantRepository;
        _chatNotificationService = chatNotificationService;
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<Result<List<ReactionResponse>>> Handle(ReactMessageCommand request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Emoji))
            return Result<List<ReactionResponse>>.Failure(new Error("Reaction.InvalidEmoji", "Biểu tượng cảm xúc không hợp lệ."));

        var message = await _messageRepository.GetByIdWithDetailsAsync(request.MessageId);
        if (message == null)
            return Result<List<ReactionResponse>>.Failure(DomainErrors.Message.NotFound);

        if (message.IsDeleted || message.IsRecalled)
            return Result<List<ReactionResponse>>.Failure(new Error("Reaction.CannotReact", "Không thể thả biểu cảm cho tin nhắn đã bị xóa hoặc thu hồi."));

        var isParticipant = await _conversationRepository.IsParticipantAsync(message.ConversationId, request.UserId);
        if (!isParticipant)
            return Result<List<ReactionResponse>>.Failure(DomainErrors.Conversation.AccessDenied);

        var existingReaction = await _reactionRepository.FindByUserAsync(request.MessageId, request.UserId);

        if (existingReaction != null)
        {
            if (existingReaction.EmojiCode == request.Emoji)
            {
                // Toggle off / remove
                _reactionRepository.Remove(existingReaction);
            }
            else
            {
                // Switch emoji
                existingReaction.EmojiCode = request.Emoji;
                existingReaction.CreatedAt = DateTime.UtcNow;
            }
        }
        else
        {
            var newReaction = new MessageReaction
            {
                MessageId = request.MessageId,
                UserId = request.UserId,
                EmojiCode = request.Emoji,
                CreatedAt = DateTime.UtcNow
            };
            await _reactionRepository.AddAsync(newReaction);
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var updatedMessage = await _messageRepository.GetByIdWithDetailsAsync(request.MessageId);
        var mapped = _mapper.Map<MessageResponse>(updatedMessage);
        var reactions = mapped.Reactions ?? new List<ReactionResponse>();

        var participants = await _participantRepository.GetConversationParticipantsAsync(message.ConversationId);
        var participantIds = participants.Select(p => p.UserId).ToList();

        await _chatNotificationService.SendMessageReactionAsync(message.ConversationId, message.Id, reactions, participantIds);

        return Result<List<ReactionResponse>>.Success(reactions);
    }
}
