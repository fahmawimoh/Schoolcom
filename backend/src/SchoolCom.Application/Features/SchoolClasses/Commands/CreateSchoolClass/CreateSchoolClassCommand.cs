using FluentValidation;
using MediatR;
using SchoolCom.Application.Common;
using SchoolCom.Application.Common.Exceptions;
using SchoolCom.Application.Common.Interfaces;
using SchoolCom.Application.Features.ClassSections;
using SchoolCom.Domain.Entities;

namespace SchoolCom.Application.Features.SchoolClasses.Commands.CreateSchoolClass;

public record CreateSchoolClassCommand(
    string NameEn,
    string NameAr,
    string? DescriptionEn,
    string? DescriptionAr,
    int SortOrder,
    bool IsActive = true,
    IReadOnlyList<Guid>? SectionIds = null) : IRequest<SchoolClassDto>;

public class CreateSchoolClassCommandValidator : AbstractValidator<CreateSchoolClassCommand>
{
    public CreateSchoolClassCommandValidator()
    {
        RuleFor(x => x.NameEn).NotEmpty().MaximumLength(100);
        RuleFor(x => x.NameAr).NotEmpty().MaximumLength(100);
        RuleFor(x => x.DescriptionEn).MaximumLength(500);
        RuleFor(x => x.DescriptionAr).MaximumLength(500);
        RuleFor(x => x.SortOrder).GreaterThanOrEqualTo(0);
        RuleFor(x => x.SectionIds).NotEmpty();
    }
}

public class CreateSchoolClassCommandHandler(
    ISchoolClassRepository repo,
    IClassSectionRepository classSectionRepo,
    ICurrentUserService currentUser,
    IAuditService audit) : IRequestHandler<CreateSchoolClassCommand, SchoolClassDto>
{
    public async Task<SchoolClassDto> Handle(CreateSchoolClassCommand request, CancellationToken ct)
    {
        var tenantId = currentUser.TenantId
            ?? throw new UnauthorizedAccessException("Tenant not found");

        SchoolClassSectionValidator.EnsureSectionsProvided(request.SectionIds);
        SchoolClassSectionValidator.EnsureNoDuplicateSectionIds(request.SectionIds!);

        if (await repo.NameExistsAsync(tenantId, request.NameEn, excludeId: null, ct))
        {
            throw new ConflictException(
                AcademicErrorCodes.SchoolClassNameDuplicate,
                AcademicErrorMessages.SchoolClassNameDuplicateEn,
                AcademicErrorMessages.SchoolClassNameDuplicateAr,
                AcademicTranslationKeys.SchoolClassNameDuplicate);
        }

        if (!await classSectionRepo.AllSectionsExistAsync(tenantId, request.SectionIds!, ct))
            throw new KeyNotFoundException("One or more sections were not found");

        var entity = new SchoolClass
        {
            TenantId = tenantId,
            NameEn = request.NameEn.Trim(),
            NameAr = request.NameAr.Trim(),
            DescriptionEn = string.IsNullOrWhiteSpace(request.DescriptionEn) ? null : request.DescriptionEn.Trim(),
            DescriptionAr = string.IsNullOrWhiteSpace(request.DescriptionAr) ? null : request.DescriptionAr.Trim(),
            SortOrder = request.SortOrder,
            IsActive = request.IsActive
        };

        await repo.AddAsync(entity, ct);
        await repo.SaveChangesAsync(ct);

        await classSectionRepo.SyncLinksAsync(tenantId, entity.Id, request.SectionIds!, ct);
        await repo.SaveChangesAsync(ct);
        await audit.LogAsync("Create", "SchoolClass", entity.Id, ct: ct);

        var created = await repo.GetByIdAsync(tenantId, entity.Id, ct)
            ?? throw new InvalidOperationException("Failed to load created school class");

        return SchoolClassMapper.ToDto(created);
    }
}
