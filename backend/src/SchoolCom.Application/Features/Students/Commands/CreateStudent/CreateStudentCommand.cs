using FluentValidation;
using MediatR;
using SchoolCom.Application.Common.Interfaces;
using SchoolCom.Domain.Entities;

namespace SchoolCom.Application.Features.Students.Commands.CreateStudent;

public record CreateStudentCommand(
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
    bool IsActive = true) : IRequest<StudentDto>;

public class CreateStudentCommandValidator : AbstractValidator<CreateStudentCommand>
{
    public CreateStudentCommandValidator()
    {
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

public class CreateStudentCommandHandler(
    IStudentRepository students,
    ICurrentUserService currentUser,
    IAuditService audit) : IRequestHandler<CreateStudentCommand, StudentDto>
{
    public async Task<StudentDto> Handle(CreateStudentCommand request, CancellationToken cancellationToken)
    {
        var tenantId = currentUser.TenantId
            ?? throw new UnauthorizedAccessException("Tenant not found");

        if (!string.IsNullOrWhiteSpace(request.RequestedStudentNumber))
            throw new InvalidOperationException("Student ID is system-generated and cannot be specified.");

        if (!await students.SectionBelongsToSchoolClassAsync(tenantId, request.SchoolClassId, request.SectionId, cancellationToken))
            throw new InvalidOperationException("Invalid school class and section combination");

        var studentNumber = await students.GenerateNextStudentNumberAsync(tenantId, cancellationToken);

        var student = new Student
        {
            TenantId = tenantId,
            StudentNumber = studentNumber,
            FirstNameEn = request.FirstNameEn.Trim(),
            FirstNameAr = request.FirstNameAr.Trim(),
            LastNameEn = request.LastNameEn.Trim(),
            LastNameAr = request.LastNameAr.Trim(),
            SchoolClassId = request.SchoolClassId,
            SectionId = request.SectionId,
            ParentId = request.ParentId,
            ParentName = request.ParentName.Trim(),
            ParentPhone = request.ParentPhone.Trim(),
            ParentEmail = string.IsNullOrWhiteSpace(request.ParentEmail) ? null : request.ParentEmail.Trim(),
            IsActive = request.IsActive
        };

        await students.AddAsync(student, cancellationToken);
        await students.SaveChangesAsync(cancellationToken);
        await audit.LogAsync("Create", "Student", student.Id, ct: cancellationToken);

        var created = await students.GetByIdAsync(tenantId, student.Id, cancellationToken)
            ?? throw new InvalidOperationException("Failed to load created student");

        return StudentMapper.ToDto(created, currentUser.PreferredLanguage);
    }
}
