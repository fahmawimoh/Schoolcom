using FluentValidation;
using MediatR;
using SchoolCom.Application.Common;
using SchoolCom.Application.Common.Exceptions;
using SchoolCom.Application.Common.Interfaces;
using SchoolCom.Domain.Entities;

namespace SchoolCom.Application.Features.Sections.Commands.CreateSection;

public record CreateSectionCommand(
    Guid SchoolClassId,
    string NameEn,
    string NameAr,
    string? DescriptionEn,
    string? DescriptionAr,
    bool IsActive = true) : IRequest<SectionDto>;

public class CreateSectionCommandValidator : AbstractValidator<CreateSectionCommand>
{
    public CreateSectionCommandValidator()
    {
        RuleFor(x => x.SchoolClassId).NotEmpty();
        RuleFor(x => x.NameEn).NotEmpty().MaximumLength(100);
        RuleFor(x => x.NameAr).NotEmpty().MaximumLength(100);
        RuleFor(x => x.DescriptionEn).MaximumLength(500);
        RuleFor(x => x.DescriptionAr).MaximumLength(500);
    }
}

public class CreateSectionCommandHandler(
    ISectionRepository repo,
    ICurrentUserService currentUser,
    IAuditService audit) : IRequestHandler<CreateSectionCommand, SectionDto>
{
    public async Task<SectionDto> Handle(CreateSectionCommand request, CancellationToken ct)
    {
        var tenantId = currentUser.TenantId
            ?? throw new UnauthorizedAccessException("Tenant not found");

        if (!await repo.SchoolClassExistsAsync(tenantId, request.SchoolClassId, ct))
            throw new KeyNotFoundException("School class not found");

        if (await repo.ClassSectionExistsAsync(
                tenantId, request.SchoolClassId, request.NameEn, excludeSectionId: null, ct))
        {
            throw new ConflictException(
                AcademicErrorCodes.ClassSectionDuplicate,
                AcademicErrorMessages.ClassSectionDuplicateEn,
                AcademicErrorMessages.ClassSectionDuplicateAr,
                AcademicTranslationKeys.ClassSectionDuplicate);
        }

        var entity = new Section
        {
            TenantId = tenantId,
            SchoolClassId = request.SchoolClassId,
            NameEn = request.NameEn.Trim(),
            NameAr = request.NameAr.Trim(),
            DescriptionEn = string.IsNullOrWhiteSpace(request.DescriptionEn) ? null : request.DescriptionEn.Trim(),
            DescriptionAr = string.IsNullOrWhiteSpace(request.DescriptionAr) ? null : request.DescriptionAr.Trim(),
            IsActive = request.IsActive
        };

        await repo.AddAsync(entity, ct);
        await repo.SaveChangesAsync(ct);
        await audit.LogAsync("Create", "Section", entity.Id, ct: ct);

        var created = await repo.GetByIdAsync(tenantId, entity.Id, ct)
            ?? throw new InvalidOperationException("Failed to load created section");

        return new SectionDto(
            created.Id,
            created.SchoolClassId,
            created.SchoolClass.NameEn,
            created.SchoolClass.NameAr,
            created.NameEn,
            created.NameAr,
            created.DescriptionEn,
            created.DescriptionAr,
            created.IsActive,
            created.Students.Count,
            created.CreatedAt);
    }
}
