using FluentValidation;
using MediatR;
using SchoolCom.Application.Common.Interfaces;

namespace SchoolCom.Application.Features.Rooms.Queries.GetRooms;

public record GetRoomsQuery(
    int Page = 1,
    int PageSize = 10,
    string? Search = null,
    bool? IsActive = null,
    string? SortBy = null,
    bool SortDesc = false) : IRequest<RoomListResult>;

public class GetRoomsQueryValidator : AbstractValidator<GetRoomsQuery>
{
    public GetRoomsQueryValidator()
    {
        RuleFor(x => x.Page).GreaterThanOrEqualTo(1);
        RuleFor(x => x.PageSize).InclusiveBetween(1, 100);
    }
}

public class GetRoomsQueryHandler(
    IRoomRepository repo,
    ICurrentUserService currentUser) : IRequestHandler<GetRoomsQuery, RoomListResult>
{
    public async Task<RoomListResult> Handle(GetRoomsQuery request, CancellationToken ct)
    {
        var tenantId = currentUser.TenantId
            ?? throw new UnauthorizedAccessException("Tenant not found");

        return await repo.GetPagedAsync(
            tenantId,
            request.Page,
            request.PageSize,
            request.Search,
            request.IsActive,
            request.SortBy,
            request.SortDesc,
            ct);
    }
}
