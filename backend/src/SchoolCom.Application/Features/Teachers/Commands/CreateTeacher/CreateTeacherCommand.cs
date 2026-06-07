using FluentValidation;
using MediatR;
using SchoolCom.Application.Common.Interfaces;
using SchoolCom.Domain.Entities;

namespace SchoolCom.Application.Features.Teachers.Commands.CreateTeacher;

public record CreateTeacherCommand(
    string FirstNameEn,
    string FirstNameAr,
    string LastNameEn,
    string LastNameAr,
    string Email,
    string Phone,
    string EmployeeNumber,
    string SubjectEn,
    string SubjectAr,
    bool IsActive = true,
    Guid? UserId = null) : IRequest<TeacherDto>;

public class CreateTeacherCommandValidator : AbstractValidator<CreateTeacherCommand>
{
    public CreateTeacherCommandValidator()
    {
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

public class CreateTeacherCommandHandler(
    ITeacherRepository repo,
    ICurrentUserService currentUser,
    IAuditService audit) : IRequestHandler<CreateTeacherCommand, TeacherDto>
{
    public async Task<TeacherDto> Handle(CreateTeacherCommand request, CancellationToken ct)
    {
        var tenantId = currentUser.TenantId
            ?? throw new UnauthorizedAccessException("Tenant not found");

        var employeeNumber = request.EmployeeNumber.Trim();
        if (await repo.EmployeeNumberExistsAsync(tenantId, employeeNumber, null, ct))
            throw new InvalidOperationException("Employee number is already in use");

        if (request.UserId.HasValue &&
            !await repo.UserBelongsToTenantAsync(tenantId, request.UserId.Value, ct))
            throw new InvalidOperationException("Invalid user");

        var entity = new Teacher
        {
            TenantId = tenantId,
            FirstNameEn = request.FirstNameEn.Trim(),
            FirstNameAr = request.FirstNameAr.Trim(),
            LastNameEn = request.LastNameEn.Trim(),
            LastNameAr = request.LastNameAr.Trim(),
            Email = request.Email.Trim(),
            Phone = request.Phone.Trim(),
            EmployeeNumber = employeeNumber,
            SubjectEn = request.SubjectEn.Trim(),
            SubjectAr = request.SubjectAr.Trim(),
            IsActive = request.IsActive,
            UserId = request.UserId
        };

        await repo.AddAsync(entity, ct);
        await repo.SaveChangesAsync(ct);
        await audit.LogAsync("Create", "Teacher", entity.Id, ct: ct);

        return TeacherMapper.ToDto(entity);
    }
}
