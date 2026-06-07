using FluentValidation;
using MediatR;
using SchoolCom.Application.Common.Interfaces;

namespace SchoolCom.Application.Features.Students.Queries.GetStudents;

public record GetStudentsQuery(
    int Page = 1,
    int PageSize = 5,
    Guid? SchoolClassId = null,
    Guid? SectionId = null,
    string? Search = null,
    string? Lang = null) : IRequest<StudentListResult>;

public class GetStudentsQueryValidator : AbstractValidator<GetStudentsQuery>
{
    public GetStudentsQueryValidator()
    {
        RuleFor(x => x.Page).GreaterThanOrEqualTo(1);
        RuleFor(x => x.PageSize).InclusiveBetween(1, 100);
    }
}

public class GetStudentsQueryHandler(
    IStudentRepository students,
    ICurrentUserService currentUser) : IRequestHandler<GetStudentsQuery, StudentListResult>
{
    public async Task<StudentListResult> Handle(GetStudentsQuery request, CancellationToken cancellationToken)
    {
        var tenantId = currentUser.TenantId
            ?? throw new UnauthorizedAccessException("Tenant not found");

        var locale = request.Lang ?? currentUser.PreferredLanguage;

        return await students.GetPagedAsync(
            tenantId,
            request.Page,
            request.PageSize,
            request.SchoolClassId,
            request.SectionId,
            request.Search,
            locale,
            cancellationToken);
    }
}
