using FluentValidation;
using MediatR;
using SchoolCom.Application.Common.Interfaces;

namespace SchoolCom.Application.Features.Subjects.Queries.GetSubjects;

public record GetSubjectsQuery(
    int Page = 1,
    int PageSize = 10,
    string? Search = null,
    bool? IsActive = null,
    string? SortBy = null,
    bool SortDesc = false) : IRequest<SubjectListResult>;

public class GetSubjectsQueryValidator : AbstractValidator<GetSubjectsQuery>
{
    public GetSubjectsQueryValidator()
    {
        RuleFor(x => x.Page).GreaterThanOrEqualTo(1);
        RuleFor(x => x.PageSize).InclusiveBetween(1, 100);
    }
}

public class GetSubjectsQueryHandler(
    ISubjectRepository repo,
    ICurrentUserService currentUser) : IRequestHandler<GetSubjectsQuery, SubjectListResult>
{
    public async Task<SubjectListResult> Handle(GetSubjectsQuery request, CancellationToken ct)
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
