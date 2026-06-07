using MediatR;
using SchoolCom.Application.Common.Interfaces;

namespace SchoolCom.Application.Features.Subjects.Queries.GetSubjectById;

public record GetSubjectByIdQuery(Guid Id) : IRequest<SubjectDto>;

public class GetSubjectByIdQueryHandler(
    ISubjectRepository repo,
    ICurrentUserService currentUser) : IRequestHandler<GetSubjectByIdQuery, SubjectDto>
{
    public async Task<SubjectDto> Handle(GetSubjectByIdQuery request, CancellationToken ct)
    {
        var tenantId = currentUser.TenantId
            ?? throw new UnauthorizedAccessException("Tenant not found");

        var entity = await repo.GetByIdAsync(tenantId, request.Id, ct)
            ?? throw new KeyNotFoundException("Subject not found");

        return new SubjectDto(entity.Id, entity.NameEn, entity.NameAr, entity.Code, entity.IsActive, entity.CreatedAt);
    }
}
