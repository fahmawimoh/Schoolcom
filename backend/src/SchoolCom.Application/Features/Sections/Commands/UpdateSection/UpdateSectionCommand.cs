using FluentValidation;
using MediatR;
using SchoolCom.Application.Common;
using SchoolCom.Application.Common.Exceptions;
using SchoolCom.Application.Common.Interfaces;

namespace SchoolCom.Application.Features.Sections.Commands.UpdateSection;

public record UpdateSectionCommand(
    Guid Id,
    string NameEn,
    string NameAr,
    string? DescriptionEn,
    string? DescriptionAr,
    bool IsActive) : IRequest<SectionDto>;

public class UpdateSectionCommandValidator : AbstractValidator<UpdateSectionCommand>
{
    public UpdateSectionCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
        RuleFor(x => x.NameEn).NotEmpty().MaximumLength(100);
        RuleFor(x => x.NameAr).NotEmpty().MaximumLength(100);
        RuleFor(x => x.DescriptionEn).MaximumLength(500);
        RuleFor(x => x.DescriptionAr).MaximumLength(500);
    }
}

public class UpdateSectionCommandHandler(
    ISectionRepository repo,
    ICurrentUserService currentUser,
    IAuditService audit) : IRequestHandler<UpdateSectionCommand, SectionDto>
{
    public async Task<SectionDto> Handle(UpdateSectionCommand request, CancellationToken ct)
    {
        var tenantId = currentUser.TenantId
            ?? throw new UnauthorizedAccessException("Tenant not found");

        var entity = await repo.GetByIdForUpdateAsync(tenantId, request.Id, ct)
            ?? throw new KeyNotFoundException("Section not found");

        if (await repo.NameExistsAsync(tenantId, request.NameEn, excludeSectionId: entity.Id, ct))
        {
            throw new ConflictException(
                AcademicErrorCodes.SectionNameDuplicate,
                AcademicErrorMessages.SectionNameDuplicateEn,
                AcademicErrorMessages.SectionNameDuplicateAr,
                AcademicTranslationKeys.SectionNameDuplicate);
        }

        entity.NameEn = request.NameEn.Trim();
        entity.NameAr = request.NameAr.Trim();
        entity.DescriptionEn = string.IsNullOrWhiteSpace(request.DescriptionEn) ? null : request.DescriptionEn.Trim();
        entity.DescriptionAr = string.IsNullOrWhiteSpace(request.DescriptionAr) ? null : request.DescriptionAr.Trim();
        entity.IsActive = request.IsActive;

        await repo.SaveChangesAsync(ct);
        await audit.LogAsync("Update", "Section", entity.Id, ct: ct);

        var updated = await repo.GetByIdAsync(tenantId, entity.Id, ct)
            ?? throw new InvalidOperationException("Failed to load updated section");

        return new SectionDto(
            updated.Id,
            updated.NameEn,
            updated.NameAr,
            updated.DescriptionEn,
            updated.DescriptionAr,
            updated.IsActive,
            updated.ClassSections.Count,
            updated.CreatedAt);
    }
}
