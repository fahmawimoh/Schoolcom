using MediatR;
using SchoolCom.Application.Common.Interfaces;

namespace SchoolCom.Application.Features.Messaging.Queries.GetLinkedStudents;

public record GetLinkedStudentsQuery : IRequest<IReadOnlyList<LinkedStudentDto>>;

public class GetLinkedStudentsQueryHandler(
    IMessagingRepository repository,
    ICurrentUserService currentUser,
    IMessagingAccessService access) : IRequestHandler<GetLinkedStudentsQuery, IReadOnlyList<LinkedStudentDto>>
{
    public Task<IReadOnlyList<LinkedStudentDto>> Handle(
        GetLinkedStudentsQuery request,
        CancellationToken cancellationToken)
    {
        var tenantId = access.RequireTenantId();
        var userId = access.RequireUserId();
        var isTeacher = MessagingAuthorization.IsTeacher(currentUser.Roles);

        return repository.GetLinkedStudentsForUserAsync(
            tenantId, userId, isTeacher, cancellationToken);
    }
}
