using SchoolCom.Application.Common.Interfaces;

namespace SchoolCom.Application.Features.Messaging;

public static class MessagingAuthorization
{
    public static bool IsAdmin(IReadOnlyList<string> roles) =>
        roles.Contains("SchoolAdmin") || roles.Contains("SuperAdmin");

    public static bool IsTeacher(IReadOnlyList<string> roles) => roles.Contains("Teacher");

    public static bool IsParent(IReadOnlyList<string> roles) => roles.Contains("Parent");
}

public class MessagingAccessService(
    IMessagingRepository repository,
    ICurrentUserService currentUser) : IMessagingAccessService
{
    public async Task EnsureCanAccessConversationAsync(Guid conversationId, CancellationToken ct)
    {
        var tenantId = currentUser.TenantId
            ?? throw new UnauthorizedAccessException("Tenant not found");
        var userId = currentUser.UserId
            ?? throw new UnauthorizedAccessException("User not found");

        if (MessagingAuthorization.IsAdmin(currentUser.Roles))
            return;

        if (!await repository.IsParticipantAsync(tenantId, conversationId, userId, ct))
            throw new UnauthorizedAccessException("You do not have access to this conversation.");
    }

    public Guid RequireUserId() =>
        currentUser.UserId ?? throw new UnauthorizedAccessException("User not found");

    public Guid RequireTenantId() =>
        currentUser.TenantId ?? throw new UnauthorizedAccessException("Tenant not found");
}

public interface IMessagingAccessService
{
    Task EnsureCanAccessConversationAsync(Guid conversationId, CancellationToken ct);
    Guid RequireUserId();
    Guid RequireTenantId();
}
