using FluentValidation;
using MediatR;
using SchoolCom.Application.Common.Interfaces;
using SchoolCom.Domain.Entities;

namespace SchoolCom.Application.Features.SchoolClasses.Commands.UpdateSchoolClass;

public record UpdateSchoolClassCommand(
    Guid Id,
    string NameEn,
    string NameAr,
    string? DescriptionEn,
    string? DescriptionAr,
    int SortOrder,
    bool IsActive,
    IReadOnlyList<SchoolClassSectionInput>? Sections = null) : IRequest<SchoolClassDto>;

public class UpdateSchoolClassCommandValidator : AbstractValidator<UpdateSchoolClassCommand>
{
    public UpdateSchoolClassCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
        RuleFor(x => x.NameEn).NotEmpty().MaximumLength(100);
        RuleFor(x => x.NameAr).NotEmpty().MaximumLength(100);
        RuleFor(x => x.DescriptionEn).MaximumLength(500);
        RuleFor(x => x.DescriptionAr).MaximumLength(500);
        RuleFor(x => x.SortOrder).GreaterThanOrEqualTo(0);
        RuleForEach(x => x.Sections).ChildRules(section =>
        {
            section.RuleFor(s => s.NameEn).NotEmpty().MaximumLength(100);
            section.RuleFor(s => s.NameAr).NotEmpty().MaximumLength(100);
        });
    }
}

public class UpdateSchoolClassCommandHandler(
    ISchoolClassRepository repo,
    ICurrentUserService currentUser,
    IAuditService audit) : IRequestHandler<UpdateSchoolClassCommand, SchoolClassDto>
{
    public async Task<SchoolClassDto> Handle(UpdateSchoolClassCommand request, CancellationToken ct)
    {
        var tenantId = currentUser.TenantId
            ?? throw new UnauthorizedAccessException("Tenant not found");

        var entity = await repo.GetByIdWithSectionsForUpdateAsync(tenantId, request.Id, ct)
            ?? throw new KeyNotFoundException("School class not found");

        SchoolClassSectionValidator.EnsureNoDuplicateNamesInRequest(request.Sections);

        if (request.Sections is { Count: > 0 })
        {
            SchoolClassSectionValidator.EnsureNoDuplicateAgainstExisting(
                request.Sections,
                entity.Sections.Select(s => s.NameEn));
        }

        entity.NameEn = request.NameEn.Trim();
        entity.NameAr = request.NameAr.Trim();
        entity.DescriptionEn = string.IsNullOrWhiteSpace(request.DescriptionEn) ? null : request.DescriptionEn.Trim();
        entity.DescriptionAr = string.IsNullOrWhiteSpace(request.DescriptionAr) ? null : request.DescriptionAr.Trim();
        entity.SortOrder = request.SortOrder;
        entity.IsActive = request.IsActive;

        if (request.Sections is { Count: > 0 })
        {
            foreach (var section in request.Sections)
            {
                entity.Sections.Add(new Section
                {
                    TenantId = tenantId,
                    SchoolClassId = entity.Id,
                    NameEn = section.NameEn.Trim(),
                    NameAr = section.NameAr.Trim(),
                    IsActive = true
                });
            }
        }

        await repo.SaveChangesAsync(ct);
        await audit.LogAsync("Update", "SchoolClass", entity.Id, ct: ct);

        var updated = await repo.GetByIdAsync(tenantId, entity.Id, ct)
            ?? throw new InvalidOperationException("Failed to load updated school class");

        return new SchoolClassDto(
            updated.Id, updated.NameEn, updated.NameAr, updated.DescriptionEn, updated.DescriptionAr,
            updated.SortOrder, updated.IsActive, updated.Sections.Count, updated.Students.Count, updated.CreatedAt);
    }
}
