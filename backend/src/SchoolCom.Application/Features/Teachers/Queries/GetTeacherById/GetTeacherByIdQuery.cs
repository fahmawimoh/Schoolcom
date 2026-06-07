using MediatR;
using SchoolCom.Application.Common.Interfaces;

namespace SchoolCom.Application.Features.Teachers.Queries.GetTeacherById;

public record GetTeacherByIdQuery(Guid Id) : IRequest<TeacherDto>;

public class GetTeacherByIdQueryHandler(
    ITeacherRepository repo,
    ICurrentUserService currentUser) : IRequestHandler<GetTeacherByIdQuery, TeacherDto>
{
    public async Task<TeacherDto> Handle(GetTeacherByIdQuery request, CancellationToken ct)
    {
        var tenantId = currentUser.TenantId
            ?? throw new UnauthorizedAccessException("Tenant not found");

        var entity = await repo.GetByIdAsync(tenantId, request.Id, ct)
            ?? throw new KeyNotFoundException("Teacher not found");

        return TeacherMapper.ToDto(entity);
    }
}
