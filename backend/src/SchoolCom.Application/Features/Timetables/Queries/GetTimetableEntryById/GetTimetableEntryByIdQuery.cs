using MediatR;
using SchoolCom.Application.Common.Interfaces;

namespace SchoolCom.Application.Features.Timetables.Queries.GetTimetableEntryById;

public record GetTimetableEntryByIdQuery(Guid Id) : IRequest<TimetableEntryDto>;

public class GetTimetableEntryByIdQueryHandler(
    ITimetableRepository repo,
    ICurrentUserService currentUser) : IRequestHandler<GetTimetableEntryByIdQuery, TimetableEntryDto>
{
    public async Task<TimetableEntryDto> Handle(GetTimetableEntryByIdQuery request, CancellationToken ct)
    {
        var tenantId = currentUser.TenantId
            ?? throw new UnauthorizedAccessException("Tenant not found");

        var entity = await repo.GetByIdAsync(tenantId, request.Id, ct)
            ?? throw new KeyNotFoundException("Timetable entry not found");

        return TimetableMapper.ToDto(entity);
    }
}
