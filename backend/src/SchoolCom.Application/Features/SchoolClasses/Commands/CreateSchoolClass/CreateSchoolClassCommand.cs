using FluentValidation;
using MediatR;
using SchoolCom.Application.Common.Interfaces;
using SchoolCom.Domain.Entities;

namespace SchoolCom.Application.Features.SchoolClasses.Commands.CreateSchoolClass;

public record CreateSchoolClassCommand(
    string NameEn,
    string NameAr,
    string? DescriptionEn,
    string? DescriptionAr,
    int SortOrder,
    bool IsActive = true,
    IReadOnlyList<SchoolClassSectionInput>? Sections = null) : IRequest<SchoolClassDto>;

public class CreateSchoolClassCommandValidator : AbstractValidator<CreateSchoolClassCommand>
{
    public CreateSchoolClassCommandValidator()
    {
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

public class CreateSchoolClassCommandHandler(
    ISchoolClassRepository repo,
    ICurrentUserService currentUser,
    IAuditService audit) : IRequestHandler<CreateSchoolClassCommand, SchoolClassDto>
{
    public async Task<SchoolClassDto> Handle(CreateSchoolClassCommand request, CancellationToken ct)
    {
        var tenantId = currentUser.TenantId
            ?? throw new UnauthorizedAccessException("Tenant not found");

        SchoolClassSectionValidator.EnsureNoDuplicateNamesInRequest(request.Sections);

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

        if (request.Sections is { Count: > 0 })
        {
            foreach (var section in request.Sections)
            {
                entity.Sections.Add(new Section
                {
                    TenantId = tenantId,
                    NameEn = section.NameEn.Trim(),
                    NameAr = section.NameAr.Trim(),
                    IsActive = true
                });
            }
        }

        await repo.AddAsync(entity, ct);
        await repo.SaveChangesAsync(ct);
        await audit.LogAsync("Create", "SchoolClass", entity.Id, ct: ct);

        return new SchoolClassDto(
            entity.Id, entity.NameEn, entity.NameAr, entity.DescriptionEn, entity.DescriptionAr,
            entity.SortOrder, entity.IsActive, entity.Sections.Count, 0, entity.CreatedAt);
    }
}
