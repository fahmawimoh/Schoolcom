using FluentValidation;
using MediatR;
using SchoolCom.Application.Common.Interfaces;

namespace SchoolCom.Application.Features.Subjects.Commands.UpdateSubject;

public record UpdateSubjectCommand(
    Guid Id,
    string NameEn,
    string NameAr,
    string? Code,
    bool IsActive) : IRequest<SubjectDto>;

public class UpdateSubjectCommandValidator : AbstractValidator<UpdateSubjectCommand>
{
    public UpdateSubjectCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
        RuleFor(x => x.NameEn).NotEmpty().MaximumLength(100);
        RuleFor(x => x.NameAr).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Code).MaximumLength(32);
    }
}

public class UpdateSubjectCommandHandler(
    ISubjectRepository repo,
    ICurrentUserService currentUser,
    IAuditService audit) : IRequestHandler<UpdateSubjectCommand, SubjectDto>
{
    public async Task<SubjectDto> Handle(UpdateSubjectCommand request, CancellationToken ct)
    {
        var tenantId = currentUser.TenantId
            ?? throw new UnauthorizedAccessException("Tenant not found");

        var entity = await repo.GetByIdForUpdateAsync(tenantId, request.Id, ct)
            ?? throw new KeyNotFoundException("Subject not found");

        entity.NameEn = request.NameEn.Trim();
        entity.NameAr = request.NameAr.Trim();
        entity.Code = string.IsNullOrWhiteSpace(request.Code) ? null : request.Code.Trim();
        entity.IsActive = request.IsActive;

        await repo.SaveChangesAsync(ct);
        await audit.LogAsync("Update", "Subject", entity.Id, ct: ct);

        return new SubjectDto(entity.Id, entity.NameEn, entity.NameAr, entity.Code, entity.IsActive, entity.CreatedAt);
    }
}
