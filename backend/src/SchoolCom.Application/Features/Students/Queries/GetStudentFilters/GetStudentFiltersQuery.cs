using MediatR;
using SchoolCom.Application.Common.Interfaces;

namespace SchoolCom.Application.Features.Students.Queries.GetStudentFilters;

public record GetStudentFiltersQuery() : IRequest<StudentFiltersDto>;

public class GetStudentFiltersQueryHandler(
    IStudentRepository students,
    ICurrentUserService currentUser) : IRequestHandler<GetStudentFiltersQuery, StudentFiltersDto>
{
    public async Task<StudentFiltersDto> Handle(GetStudentFiltersQuery request, CancellationToken cancellationToken)
    {
        var tenantId = currentUser.TenantId
            ?? throw new UnauthorizedAccessException("Tenant not found");

        return await students.GetFiltersAsync(tenantId, cancellationToken);
    }
}
