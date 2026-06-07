using FluentValidation;
using MediatR;
using SchoolCom.Application.Common.Interfaces;

namespace SchoolCom.Application.Features.SchoolClasses.Queries.GetSchoolClasses;

public record GetSchoolClassesQuery(
    int Page = 1,
    int PageSize = 10,
    string? Search = null,
    bool? IsActive = null,
    string? SortBy = null,
    bool SortDesc = false) : IRequest<SchoolClassListResult>;

public class GetSchoolClassesQueryValidator : AbstractValidator<GetSchoolClassesQuery>
{
    public GetSchoolClassesQueryValidator()
    {
        RuleFor(x => x.Page).GreaterThanOrEqualTo(1);
        RuleFor(x => x.PageSize).InclusiveBetween(1, 100);
    }
}

public class GetSchoolClassesQueryHandler(
    ISchoolClassRepository repo,
    ICurrentUserService currentUser) : IRequestHandler<GetSchoolClassesQuery, SchoolClassListResult>
{
    public async Task<SchoolClassListResult> Handle(GetSchoolClassesQuery request, CancellationToken ct)
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
