using MediatR;
using SchoolCom.Application.Common.Interfaces;

namespace SchoolCom.Application.Features.RoomAssignments.Queries.GetClassSectionOptions;

public record GetClassSectionOptionsQuery(Guid? SchoolClassId = null)
    : IRequest<IReadOnlyList<ClassSectionLookupDto>>;

public class GetClassSectionOptionsQueryHandler(
    IRoomAssignmentRepository repo,
    ICurrentUserService currentUser) : IRequestHandler<GetClassSectionOptionsQuery, IReadOnlyList<ClassSectionLookupDto>>
{
    public async Task<IReadOnlyList<ClassSectionLookupDto>> Handle(
        GetClassSectionOptionsQuery request,
        CancellationToken ct)
    {
        var tenantId = currentUser.TenantId
            ?? throw new UnauthorizedAccessException("Tenant not found");

        return await repo.GetClassSectionOptionsAsync(tenantId, request.SchoolClassId, ct);
    }
}
