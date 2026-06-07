using MediatR;
using SchoolCom.Application.Common.Interfaces;

namespace SchoolCom.Application.Features.SchoolClasses.Queries.GetSchoolClassById;

public record GetSchoolClassByIdQuery(Guid Id) : IRequest<SchoolClassDto>;

public class GetSchoolClassByIdQueryHandler(
    ISchoolClassRepository repo,
    ICurrentUserService currentUser) : IRequestHandler<GetSchoolClassByIdQuery, SchoolClassDto>
{
    public async Task<SchoolClassDto> Handle(GetSchoolClassByIdQuery request, CancellationToken ct)
    {
        var tenantId = currentUser.TenantId
            ?? throw new UnauthorizedAccessException("Tenant not found");

        var entity = await repo.GetByIdAsync(tenantId, request.Id, ct)
            ?? throw new KeyNotFoundException("School class not found");

        return new SchoolClassDto(
            entity.Id, entity.NameEn, entity.NameAr, entity.DescriptionEn, entity.DescriptionAr,
            entity.SortOrder, entity.IsActive, entity.Sections.Count, entity.Students.Count, entity.CreatedAt);
    }
}
