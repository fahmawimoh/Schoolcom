using MediatR;
using SchoolCom.Application.Common.Interfaces;

namespace SchoolCom.Application.Features.Messaging.Queries.GetMessages;

public record GetMessagesQuery(
    Guid ConversationId,
    string? Cursor,
    int PageSize = 30,
    string? Search = null) : IRequest<MessageListResult>;

public class GetMessagesQueryHandler(
    IMessagingRepository repository,
    IMessagingAccessService access,
    ICurrentUserService currentUser) : IRequestHandler<GetMessagesQuery, MessageListResult>
{
    public async Task<MessageListResult> Handle(GetMessagesQuery request, CancellationToken cancellationToken)
    {
        await access.EnsureCanAccessConversationAsync(request.ConversationId, cancellationToken);

        var tenantId = access.RequireTenantId();
        var userId = access.RequireUserId();
        var isParent = MessagingAuthorization.IsParent(currentUser.Roles);
        var isAdmin = MessagingAuthorization.IsAdmin(currentUser.Roles);

        await repository.MarkDeliveredAsync(request.ConversationId, userId, cancellationToken);
        await repository.SaveChangesAsync(cancellationToken);

        return await repository.GetMessagesAsync(
            tenantId,
            request.ConversationId,
            userId,
            isParent,
            isAdmin,
            request.Cursor,
            request.PageSize,
            request.Search,
            cancellationToken);
    }
}
