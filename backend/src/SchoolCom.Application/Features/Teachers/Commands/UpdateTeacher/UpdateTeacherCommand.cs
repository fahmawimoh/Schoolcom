using FluentValidation;
using MediatR;
using SchoolCom.Application.Common.Interfaces;

namespace SchoolCom.Application.Features.Teachers.Commands.UpdateTeacher;

public record UpdateTeacherCommand(
    Guid Id,
    string FirstNameEn,
    string FirstNameAr,
    string LastNameEn,
    string LastNameAr,
    string Email,
    string Phone,
    string EmployeeNumber,
    string SubjectEn,
    string SubjectAr,
    bool IsActive,
    Guid? UserId = null) : IRequest<TeacherDto>;

public class UpdateTeacherCommandValidator : AbstractValidator<UpdateTeacherCommand>
{
    public UpdateTeacherCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
        RuleFor(x => x.FirstNameEn).NotEmpty().MaximumLength(100);
        RuleFor(x => x.FirstNameAr).NotEmpty().MaximumLength(100);
        RuleFor(x => x.LastNameEn).NotEmpty().MaximumLength(100);
        RuleFor(x => x.LastNameAr).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Email).NotEmpty().EmailAddress().MaximumLength(256);
        RuleFor(x => x.Phone).NotEmpty().MaximumLength(32);
        RuleFor(x => x.EmployeeNumber).NotEmpty().MaximumLength(50);
        RuleFor(x => x.SubjectEn).NotEmpty().MaximumLength(100);
        RuleFor(x => x.SubjectAr).NotEmpty().MaximumLength(100);
    }
}

public class UpdateTeacherCommandHandler(
    ITeacherRepository repo,
    ICurrentUserService currentUser,
    IAuditService audit) : IRequestHandler<UpdateTeacherCommand, TeacherDto>
{
    public async Task<TeacherDto> Handle(UpdateTeacherCommand request, CancellationToken ct)
    {
        var tenantId = currentUser.TenantId
            ?? throw new UnauthorizedAccessException("Tenant not found");

        var entity = await repo.GetByIdForUpdateAsync(tenantId, request.Id, ct)
            ?? throw new KeyNotFoundException("Teacher not found");

        var employeeNumber = request.EmployeeNumber.Trim();
        if (await repo.EmployeeNumberExistsAsync(tenantId, employeeNumber, request.Id, ct))
            throw new InvalidOperationException("Employee number is already in use");

        if (request.UserId.HasValue &&
            !await repo.UserBelongsToTenantAsync(tenantId, request.UserId.Value, ct))
            throw new InvalidOperationException("Invalid user");

        entity.FirstNameEn = request.FirstNameEn.Trim();
        entity.FirstNameAr = request.FirstNameAr.Trim();
        entity.LastNameEn = request.LastNameEn.Trim();
        entity.LastNameAr = request.LastNameAr.Trim();
        entity.Email = request.Email.Trim();
        entity.Phone = request.Phone.Trim();
        entity.EmployeeNumber = employeeNumber;
        entity.SubjectEn = request.SubjectEn.Trim();
        entity.SubjectAr = request.SubjectAr.Trim();
        entity.IsActive = request.IsActive;
        entity.UserId = request.UserId;

        await repo.SaveChangesAsync(ct);
        await audit.LogAsync("Update", "Teacher", entity.Id, ct: ct);

        var updated = await repo.GetByIdAsync(tenantId, entity.Id, ct)
            ?? throw new InvalidOperationException("Failed to load updated teacher");

        return TeacherMapper.ToDto(updated);
    }
}
