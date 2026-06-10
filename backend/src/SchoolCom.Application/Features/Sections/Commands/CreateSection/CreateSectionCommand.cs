using FluentValidation;
using MediatR;
using SchoolCom.Application.Common;
using SchoolCom.Application.Common.Exceptions;
using SchoolCom.Application.Common.Interfaces;
using SchoolCom.Domain.Entities;

namespace SchoolCom.Application.Features.Sections.Commands.CreateSection;

public record CreateSectionCommand(
    string NameEn,
    string NameAr,
    string? DescriptionEn,
    string? DescriptionAr,
    bool IsActive = true) : IRequest<SectionDto>;

public class CreateSectionCommandValidator : AbstractValidator<CreateSectionCommand>
{
    public CreateSectionCommandValidator()
    {
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

        if (await repo.NameExistsAsync(tenantId, request.NameEn, excludeSectionId: null, ct))
        {
            throw new ConflictException(
                AcademicErrorCodes.SectionNameDuplicate,
                AcademicErrorMessages.SectionNameDuplicateEn,
                AcademicErrorMessages.SectionNameDuplicateAr,
                AcademicTranslationKeys.SectionNameDuplicate);
        }

        var entity = new Section
        {
            TenantId = tenantId,
            NameEn = request.NameEn.Trim(),
            NameAr = request.NameAr.Trim(),
            DescriptionEn = string.IsNullOrWhiteSpace(request.DescriptionEn) ? null : request.DescriptionEn.Trim(),
            DescriptionAr = string.IsNullOrWhiteSpace(request.DescriptionAr) ? null : request.DescriptionAr.Trim(),
            IsActive = request.IsActive
        };

        await repo.AddAsync(entity, ct);
        await repo.SaveChangesAsync(ct);
        await audit.LogAsync("Create", "Section", entity.Id, ct: ct);

        return new SectionDto(
            entity.Id,
            entity.NameEn,
            entity.NameAr,
            entity.DescriptionEn,
            entity.DescriptionAr,
            entity.IsActive,
            0,
            entity.CreatedAt);
    }
}
