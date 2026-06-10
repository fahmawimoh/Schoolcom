using FluentValidation;
using MediatR;
using SchoolCom.Application.Common;
using SchoolCom.Application.Common.Exceptions;
using SchoolCom.Application.Common.Interfaces;
using SchoolCom.Application.Features.ClassSections;

namespace SchoolCom.Application.Features.SchoolClasses.Commands.UpdateSchoolClass;

public record UpdateSchoolClassCommand(
    Guid Id,
    string NameEn,
    string NameAr,
    string? DescriptionEn,
    string? DescriptionAr,
    int SortOrder,
    bool IsActive,
    IReadOnlyList<Guid>? SectionIds = null) : IRequest<SchoolClassDto>;

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
        RuleFor(x => x.SectionIds).NotEmpty();
    }
}

public class UpdateSchoolClassCommandHandler(
    ISchoolClassRepository repo,
    IClassSectionRepository classSectionRepo,
    ICurrentUserService currentUser,
    IAuditService audit) : IRequestHandler<UpdateSchoolClassCommand, SchoolClassDto>
{
    public async Task<SchoolClassDto> Handle(UpdateSchoolClassCommand request, CancellationToken ct)
    {
        var tenantId = currentUser.TenantId
            ?? throw new UnauthorizedAccessException("Tenant not found");

        var entity = await repo.GetByIdForUpdateAsync(tenantId, request.Id, ct)
            ?? throw new KeyNotFoundException("School class not found");

        SchoolClassSectionValidator.EnsureSectionsProvided(request.SectionIds);
        SchoolClassSectionValidator.EnsureNoDuplicateSectionIds(request.SectionIds!);

        if (await repo.NameExistsAsync(tenantId, request.NameEn, excludeId: entity.Id, ct))
        {
            throw new ConflictException(
                AcademicErrorCodes.SchoolClassNameDuplicate,
                AcademicErrorMessages.SchoolClassNameDuplicateEn,
                AcademicErrorMessages.SchoolClassNameDuplicateAr,
                AcademicTranslationKeys.SchoolClassNameDuplicate);
        }

        if (!await classSectionRepo.AllSectionsExistAsync(tenantId, request.SectionIds!, ct))
            throw new KeyNotFoundException("One or more sections were not found");

        entity.NameEn = request.NameEn.Trim();
        entity.NameAr = request.NameAr.Trim();
        entity.DescriptionEn = string.IsNullOrWhiteSpace(request.DescriptionEn) ? null : request.DescriptionEn.Trim();
        entity.DescriptionAr = string.IsNullOrWhiteSpace(request.DescriptionAr) ? null : request.DescriptionAr.Trim();
        entity.SortOrder = request.SortOrder;
        entity.IsActive = request.IsActive;

        await classSectionRepo.SyncLinksAsync(tenantId, entity.Id, request.SectionIds!, ct);
        await repo.SaveChangesAsync(ct);
        await audit.LogAsync("Update", "SchoolClass", entity.Id, ct: ct);

        var updated = await repo.GetByIdAsync(tenantId, entity.Id, ct)
            ?? throw new InvalidOperationException("Failed to load updated school class");

        return SchoolClassMapper.ToDto(updated);
    }
}
