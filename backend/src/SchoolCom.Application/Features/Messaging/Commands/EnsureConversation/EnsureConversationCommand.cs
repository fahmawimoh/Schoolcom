using MediatR;
using SchoolCom.Application.Common.Interfaces;

namespace SchoolCom.Application.Features.Messaging.Commands.EnsureConversation;

public record EnsureConversationCommand(Guid StudentId) : IRequest<ConversationListItemDto>;

public class EnsureConversationCommandHandler(
    IMessagingRepository repository,
    ICurrentUserService currentUser,
    IMessagingAccessService access,
    IAuditService audit) : IRequestHandler<EnsureConversationCommand, ConversationListItemDto>
{
    public async Task<ConversationListItemDto> Handle(
        EnsureConversationCommand request,
        CancellationToken cancellationToken)
    {
        var tenantId = access.RequireTenantId();
        var userId = access.RequireUserId();

        Guid parentId;
        Guid teacherId;

        if (MessagingAuthorization.IsParent(currentUser.Roles))
        {
            parentId = userId;
            if (!await repository.IsParentOfStudentAsync(tenantId, parentId, request.StudentId, cancellationToken))
                throw new UnauthorizedAccessException("You are not linked to this student.");

            teacherId = await repository.GetHomeroomTeacherIdAsync(tenantId, request.StudentId, cancellationToken)
                ?? throw new InvalidOperationException("No teacher assigned to this student's class.");
        }
        else if (MessagingAuthorization.IsTeacher(currentUser.Roles))
        {
            teacherId = userId;
            var homeroom = await repository.GetHomeroomTeacherIdAsync(tenantId, request.StudentId, cancellationToken);
            if (homeroom != teacherId)
                throw new UnauthorizedAccessException("You are not the homeroom teacher for this student.");

            parentId = await repository.GetPrimaryParentIdAsync(tenantId, request.StudentId, cancellationToken)
                ?? throw new InvalidOperationException("No parent linked to this student.");
        }
        else
        {
            throw new UnauthorizedAccessException("Only parents and teachers can start conversations.");
        }

        var conversation = await repository.GetOrCreateConversationAsync(
            tenantId, request.StudentId, parentId, teacherId, cancellationToken);

        await repository.SaveChangesAsync(cancellationToken);

        await audit.LogAsync(
            "Create", "Conversation", conversation.Id, new { request.StudentId }, cancellationToken);

        var list = await repository.GetConversationsForUserAsync(
            tenantId,
            userId,
            MessagingAuthorization.IsTeacher(currentUser.Roles),
            null,
            cancellationToken);

        return list.First(c => c.Id == conversation.Id);
    }
}
