using MediatR;
using SchoolCom.Application.Common.Interfaces;

namespace SchoolCom.Application.Features.Students.Queries.GetStudentById;

public record GetStudentByIdQuery(Guid Id, string? Lang = null) : IRequest<StudentDto>;

public class GetStudentByIdQueryHandler(
    IStudentRepository students,
    ICurrentUserService currentUser) : IRequestHandler<GetStudentByIdQuery, StudentDto>
{
    public async Task<StudentDto> Handle(GetStudentByIdQuery request, CancellationToken cancellationToken)
    {
        var tenantId = currentUser.TenantId
            ?? throw new UnauthorizedAccessException("Tenant not found");

        var student = await students.GetByIdAsync(tenantId, request.Id, cancellationToken)
            ?? throw new KeyNotFoundException($"Student {request.Id} not found");

        var locale = request.Lang ?? currentUser.PreferredLanguage;
        return StudentMapper.ToDto(student, locale);
    }
}
