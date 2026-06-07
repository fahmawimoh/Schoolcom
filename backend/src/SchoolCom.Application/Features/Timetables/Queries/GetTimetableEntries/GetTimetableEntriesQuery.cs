using FluentValidation;
using MediatR;
using SchoolCom.Application.Common.Interfaces;

namespace SchoolCom.Application.Features.Timetables.Queries.GetTimetableEntries;

public record GetTimetableEntriesQuery(
    int Page = 1,
    int PageSize = 10,
    Guid? SchoolClassId = null,
    Guid? SectionId = null,
    Guid? TeacherId = null,
    Guid? SubjectId = null,
    string? SortBy = null,
    bool SortDesc = false) : IRequest<TimetableEntryListResult>;

public class GetTimetableEntriesQueryValidator : AbstractValidator<GetTimetableEntriesQuery>
{
    public GetTimetableEntriesQueryValidator()
    {
        RuleFor(x => x.Page).GreaterThanOrEqualTo(1);
        RuleFor(x => x.PageSize).InclusiveBetween(1, 100);
    }
}

public class GetTimetableEntriesQueryHandler(
    ITimetableRepository repo,
    ICurrentUserService currentUser) : IRequestHandler<GetTimetableEntriesQuery, TimetableEntryListResult>
{
    public async Task<TimetableEntryListResult> Handle(GetTimetableEntriesQuery request, CancellationToken ct)
    {
        var tenantId = currentUser.TenantId
            ?? throw new UnauthorizedAccessException("Tenant not found");

        return await repo.GetPagedAsync(
            tenantId,
            request.Page,
            request.PageSize,
            request.SchoolClassId,
            request.SectionId,
            request.TeacherId,
            request.SubjectId,
            request.SortBy,
            request.SortDesc,
            ct);
    }
}
