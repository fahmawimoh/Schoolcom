using FluentValidation;
using MediatR;
using SchoolCom.Application.Common.Interfaces;

namespace SchoolCom.Application.Features.RoomAssignments.Queries.GetRoomAssignments;

public record GetRoomAssignmentsQuery(
    int Page = 1,
    int PageSize = 10,
    Guid? SchoolClassId = null,
    Guid? SectionId = null,
    Guid? RoomId = null) : IRequest<RoomAssignmentListResult>;

public class GetRoomAssignmentsQueryValidator : AbstractValidator<GetRoomAssignmentsQuery>
{
    public GetRoomAssignmentsQueryValidator()
    {
        RuleFor(x => x.Page).GreaterThanOrEqualTo(1);
        RuleFor(x => x.PageSize).InclusiveBetween(1, 100);
    }
}

public class GetRoomAssignmentsQueryHandler(
    IRoomAssignmentRepository repo,
    ICurrentUserService currentUser) : IRequestHandler<GetRoomAssignmentsQuery, RoomAssignmentListResult>
{
    public async Task<RoomAssignmentListResult> Handle(GetRoomAssignmentsQuery request, CancellationToken ct)
    {
        var tenantId = currentUser.TenantId
            ?? throw new UnauthorizedAccessException("Tenant not found");

        return await repo.GetPagedAsync(
            tenantId,
            request.Page,
            request.PageSize,
            request.SchoolClassId,
            request.SectionId,
            request.RoomId,
            ct);
    }
}
