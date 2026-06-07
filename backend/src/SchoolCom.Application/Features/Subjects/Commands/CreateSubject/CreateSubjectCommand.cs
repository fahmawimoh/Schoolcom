using FluentValidation;
using MediatR;
using SchoolCom.Application.Common.Interfaces;
using SchoolCom.Domain.Entities;

namespace SchoolCom.Application.Features.Subjects.Commands.CreateSubject;

public record CreateSubjectCommand(
    string NameEn,
    string NameAr,
    string? Code,
    bool IsActive = true) : IRequest<SubjectDto>;

public class CreateSubjectCommandValidator : AbstractValidator<CreateSubjectCommand>
{
    public CreateSubjectCommandValidator()
    {
        RuleFor(x => x.NameEn).NotEmpty().MaximumLength(100);
        RuleFor(x => x.NameAr).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Code).MaximumLength(32);
    }
}

public class CreateSubjectCommandHandler(
    ISubjectRepository repo,
    ICurrentUserService currentUser,
    IAuditService audit) : IRequestHandler<CreateSubjectCommand, SubjectDto>
{
    public async Task<SubjectDto> Handle(CreateSubjectCommand request, CancellationToken ct)
    {
        var tenantId = currentUser.TenantId
            ?? throw new UnauthorizedAccessException("Tenant not found");

        var entity = new Subject
        {
            TenantId = tenantId,
            NameEn = request.NameEn.Trim(),
            NameAr = request.NameAr.Trim(),
            Code = string.IsNullOrWhiteSpace(request.Code) ? null : request.Code.Trim(),
            IsActive = request.IsActive
        };

        await repo.AddAsync(entity, ct);
        await repo.SaveChangesAsync(ct);
        await audit.LogAsync("Create", "Subject", entity.Id, ct: ct);

        return new SubjectDto(entity.Id, entity.NameEn, entity.NameAr, entity.Code, entity.IsActive, entity.CreatedAt);
    }
}
