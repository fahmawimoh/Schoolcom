using MediatR;

namespace SchoolCom.Application.Features.Messaging.Commands.MarkConversationRead;

public record MarkConversationReadCommand(Guid ConversationId) : IRequest;

public class MarkConversationReadCommandHandler(
    IMessagingRepository repository,
    IMessagingAccessService access) : IRequestHandler<MarkConversationReadCommand>
{
    public async Task Handle(MarkConversationReadCommand request, CancellationToken cancellationToken)
    {
        await access.EnsureCanAccessConversationAsync(request.ConversationId, cancellationToken);
        var userId = access.RequireUserId();
        await repository.MarkReadAsync(request.ConversationId, userId, cancellationToken);
        await repository.SaveChangesAsync(cancellationToken);
    }
}
