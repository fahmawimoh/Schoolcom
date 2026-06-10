using FluentValidation;
using MediatR;
using SchoolCom.Application.Common.Interfaces;
using SchoolCom.Application.Features.ClassSections;
using SchoolCom.Application.Features.SchoolClasses;

namespace SchoolCom.Application.Features.Sections.Queries.GetSectionsBySchoolClass;

public record GetSectionsBySchoolClassQuery(Guid SchoolClassId) : IRequest<IReadOnlyList<ClassSectionOptionDto>>;

public class GetSectionsBySchoolClassQueryValidator : AbstractValidator<GetSectionsBySchoolClassQuery>
{
    public GetSectionsBySchoolClassQueryValidator()
    {
        RuleFor(x => x.SchoolClassId).NotEmpty();
    }
}

public class GetSectionsBySchoolClassQueryHandler(
    IClassSectionRepository classSectionRepo,
    ISchoolClassRepository schoolClassRepo,
    ICurrentUserService currentUser) : IRequestHandler<GetSectionsBySchoolClassQuery, IReadOnlyList<ClassSectionOptionDto>>
{
    public async Task<IReadOnlyList<ClassSectionOptionDto>> Handle(
        GetSectionsBySchoolClassQuery request,
        CancellationToken ct)
    {
        var tenantId = currentUser.TenantId
            ?? throw new UnauthorizedAccessException("Tenant not found");

        if (await schoolClassRepo.GetByIdAsync(tenantId, request.SchoolClassId, ct) is null)
            throw new KeyNotFoundException("School class not found");

        return await classSectionRepo.GetBySchoolClassAsync(tenantId, request.SchoolClassId, ct);
    }
}
