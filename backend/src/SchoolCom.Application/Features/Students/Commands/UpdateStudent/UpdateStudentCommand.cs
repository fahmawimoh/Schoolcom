using FluentValidation;
using MediatR;
using SchoolCom.Application.Common.Interfaces;

namespace SchoolCom.Application.Features.Students.Commands.UpdateStudent;

public record UpdateStudentCommand(
    Guid Id,
    string? RequestedStudentNumber,
    string FirstNameEn,
    string FirstNameAr,
    string LastNameEn,
    string LastNameAr,
    Guid SchoolClassId,
    Guid SectionId,
    Guid? ParentId,
    string ParentName,
    string ParentPhone,
    string? ParentEmail,
    bool IsActive) : IRequest<StudentDto>;

public class UpdateStudentCommandValidator : AbstractValidator<UpdateStudentCommand>
{
    public UpdateStudentCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
        RuleFor(x => x.FirstNameEn).NotEmpty().MaximumLength(100);
        RuleFor(x => x.FirstNameAr).NotEmpty().MaximumLength(100);
        RuleFor(x => x.LastNameEn).NotEmpty().MaximumLength(100);
        RuleFor(x => x.LastNameAr).NotEmpty().MaximumLength(100);
        RuleFor(x => x.SchoolClassId).NotEmpty();
        RuleFor(x => x.SectionId).NotEmpty();
        RuleFor(x => x.ParentName).NotEmpty().MaximumLength(200);
        RuleFor(x => x.ParentPhone).NotEmpty().MaximumLength(32);
        RuleFor(x => x.ParentEmail).EmailAddress().When(x => !string.IsNullOrWhiteSpace(x.ParentEmail));
    }
}

public class UpdateStudentCommandHandler(
    IStudentRepository students,
    ICurrentUserService currentUser,
    IAuditService audit) : IRequestHandler<UpdateStudentCommand, StudentDto>
{
    public async Task<StudentDto> Handle(UpdateStudentCommand request, CancellationToken cancellationToken)
    {
        var tenantId = currentUser.TenantId
            ?? throw new UnauthorizedAccessException("Tenant not found");

        var student = await students.GetByIdForUpdateAsync(tenantId, request.Id, cancellationToken)
            ?? throw new KeyNotFoundException($"Student {request.Id} not found");

        if (request.RequestedStudentNumber is not null)
            throw new InvalidOperationException("Student ID cannot be modified.");

        if (!await students.SectionBelongsToSchoolClassAsync(tenantId, request.SchoolClassId, request.SectionId, cancellationToken))
            throw new InvalidOperationException("Invalid school class and section combination");

        student.FirstNameEn = request.FirstNameEn.Trim();
        student.FirstNameAr = request.FirstNameAr.Trim();
        student.LastNameEn = request.LastNameEn.Trim();
        student.LastNameAr = request.LastNameAr.Trim();
        student.SchoolClassId = request.SchoolClassId;
        student.SectionId = request.SectionId;
        student.ParentId = request.ParentId;
        student.ParentName = request.ParentName.Trim();
        student.ParentPhone = request.ParentPhone.Trim();
        student.ParentEmail = string.IsNullOrWhiteSpace(request.ParentEmail) ? null : request.ParentEmail.Trim();
        student.IsActive = request.IsActive;
        student.UpdatedAt = DateTime.UtcNow;

        await students.SaveChangesAsync(cancellationToken);
        await audit.LogAsync("Update", "Student", student.Id, ct: cancellationToken);

        var updated = await students.GetByIdAsync(tenantId, student.Id, cancellationToken)
            ?? throw new InvalidOperationException("Failed to load updated student");

        return StudentMapper.ToDto(updated, currentUser.PreferredLanguage);
    }
}
