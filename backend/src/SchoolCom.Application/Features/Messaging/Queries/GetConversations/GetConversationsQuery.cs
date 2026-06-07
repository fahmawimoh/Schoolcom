using MediatR;
using SchoolCom.Application.Common.Interfaces;

namespace SchoolCom.Application.Features.Messaging.Queries.GetConversations;

public record GetConversationsQuery(string? Search = null) : IRequest<IReadOnlyList<ConversationListItemDto>>;

public class GetConversationsQueryHandler(
    IMessagingRepository repository,
    ICurrentUserService currentUser,
    IMessagingAccessService access,
    IChatPresenceService presence) : IRequestHandler<GetConversationsQuery, IReadOnlyList<ConversationListItemDto>>
{
    public async Task<IReadOnlyList<ConversationListItemDto>> Handle(
        GetConversationsQuery request,
        CancellationToken cancellationToken)
    {
        var tenantId = access.RequireTenantId();
        var userId = access.RequireUserId();
        var isTeacher = MessagingAuthorization.IsTeacher(currentUser.Roles);

        var list = await repository.GetConversationsForUserAsync(
            tenantId, userId, isTeacher, request.Search, cancellationToken);

        return list.Select(c => c with
        {
            OtherParticipantOnline = c.OtherParticipantId.HasValue &&
                                     presence.IsOnline(c.OtherParticipantId.Value)
        }).ToList();
    }
}
