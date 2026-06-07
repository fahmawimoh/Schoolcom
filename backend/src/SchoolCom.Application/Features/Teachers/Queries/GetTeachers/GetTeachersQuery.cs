using FluentValidation;
using MediatR;
using SchoolCom.Application.Common.Interfaces;

namespace SchoolCom.Application.Features.Teachers.Queries.GetTeachers;

public record GetTeachersQuery(
    int Page = 1,
    int PageSize = 10,
    string? Search = null,
    bool? IsActive = null,
    string? Subject = null,
    string? SortBy = null,
    bool SortDesc = false) : IRequest<TeacherListResult>;

public class GetTeachersQueryValidator : AbstractValidator<GetTeachersQuery>
{
    public GetTeachersQueryValidator()
    {
        RuleFor(x => x.Page).GreaterThanOrEqualTo(1);
        RuleFor(x => x.PageSize).InclusiveBetween(1, 100);
    }
}

public class GetTeachersQueryHandler(
    ITeacherRepository repo,
    ICurrentUserService currentUser) : IRequestHandler<GetTeachersQuery, TeacherListResult>
{
    public async Task<TeacherListResult> Handle(GetTeachersQuery request, CancellationToken ct)
    {
        var tenantId = currentUser.TenantId
            ?? throw new UnauthorizedAccessException("Tenant not found");

        return await repo.GetPagedAsync(
            tenantId,
            request.Page,
            request.PageSize,
            request.Search,
            request.IsActive,
            request.Subject,
            request.SortBy,
            request.SortDesc,
            ct);
    }
}
