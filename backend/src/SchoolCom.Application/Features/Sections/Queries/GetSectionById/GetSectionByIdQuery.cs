using MediatR;
using SchoolCom.Application.Common.Interfaces;

namespace SchoolCom.Application.Features.Sections.Queries.GetSectionById;

public record GetSectionByIdQuery(Guid Id) : IRequest<SectionDto>;

public class GetSectionByIdQueryHandler(
    ISectionRepository repo,
    ICurrentUserService currentUser) : IRequestHandler<GetSectionByIdQuery, SectionDto>
{
    public async Task<SectionDto> Handle(GetSectionByIdQuery request, CancellationToken ct)
    {
        var tenantId = currentUser.TenantId
            ?? throw new UnauthorizedAccessException("Tenant not found");

        var entity = await repo.GetByIdAsync(tenantId, request.Id, ct)
            ?? throw new KeyNotFoundException("Section not found");

        return new SectionDto(
            entity.Id,
            entity.SchoolClassId,
            entity.SchoolClass.NameEn,
            entity.SchoolClass.NameAr,
            entity.NameEn,
            entity.NameAr,
            entity.DescriptionEn,
            entity.DescriptionAr,
            entity.IsActive,
            entity.Students.Count,
            entity.CreatedAt);
    }
}
