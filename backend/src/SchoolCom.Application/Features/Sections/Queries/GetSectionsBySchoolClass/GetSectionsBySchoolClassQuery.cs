using FluentValidation;
using MediatR;
using SchoolCom.Application.Common.Interfaces;

namespace SchoolCom.Application.Features.Sections.Queries.GetSectionsBySchoolClass;

public record GetSectionsBySchoolClassQuery(Guid SchoolClassId) : IRequest<IReadOnlyList<SchoolClassSectionOptionDto>>;

public class GetSectionsBySchoolClassQueryValidator : AbstractValidator<GetSectionsBySchoolClassQuery>
{
    public GetSectionsBySchoolClassQueryValidator()
    {
        RuleFor(x => x.SchoolClassId).NotEmpty();
    }
}

public class GetSectionsBySchoolClassQueryHandler(
    ISectionRepository repo,
    ICurrentUserService currentUser) : IRequestHandler<GetSectionsBySchoolClassQuery, IReadOnlyList<SchoolClassSectionOptionDto>>
{
    public async Task<IReadOnlyList<SchoolClassSectionOptionDto>> Handle(
        GetSectionsBySchoolClassQuery request,
        CancellationToken ct)
    {
        var tenantId = currentUser.TenantId
            ?? throw new UnauthorizedAccessException("Tenant not found");

        if (!await repo.SchoolClassExistsAsync(tenantId, request.SchoolClassId, ct))
            throw new KeyNotFoundException("School class not found");

        return await repo.GetSectionsBySchoolClassAsync(tenantId, request.SchoolClassId, ct);
    }
}
