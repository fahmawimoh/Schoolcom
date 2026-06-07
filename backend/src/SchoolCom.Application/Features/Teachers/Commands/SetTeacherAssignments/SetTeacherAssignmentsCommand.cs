using FluentValidation;
using MediatR;
using SchoolCom.Application.Common.Interfaces;

namespace SchoolCom.Application.Features.Teachers.Commands.SetTeacherAssignments;

public record SetTeacherAssignmentsCommand(
    Guid TeacherId,
    IReadOnlyList<TeacherAssignmentInput> Assignments) : IRequest<TeacherDto>;

public class SetTeacherAssignmentsCommandValidator : AbstractValidator<SetTeacherAssignmentsCommand>
{
    public SetTeacherAssignmentsCommandValidator()
    {
        RuleFor(x => x.TeacherId).NotEmpty();
        RuleForEach(x => x.Assignments).ChildRules(assignment =>
        {
            assignment.RuleFor(a => a.SchoolClassId).NotEmpty();
            assignment.RuleFor(a => a.SubjectId).NotEmpty();
        });
    }
}

public class SetTeacherAssignmentsCommandHandler(
    ITeacherRepository repo,
    ICurrentUserService currentUser,
    IAuditService audit) : IRequestHandler<SetTeacherAssignmentsCommand, TeacherDto>
{
    public async Task<TeacherDto> Handle(SetTeacherAssignmentsCommand request, CancellationToken ct)
    {
        var tenantId = currentUser.TenantId
            ?? throw new UnauthorizedAccessException("Tenant not found");

        var teacher = await repo.GetByIdForUpdateAsync(tenantId, request.TeacherId, ct)
            ?? throw new KeyNotFoundException("Teacher not found");

        var distinctKeys = request.Assignments
            .Select(a => (a.SchoolClassId, a.SectionId, a.SubjectId))
            .ToList();
        if (distinctKeys.Count != distinctKeys.Distinct().Count())
            throw new InvalidOperationException("Duplicate assignments are not allowed");

        foreach (var assignment in request.Assignments)
        {
            if (!await repo.SchoolClassExistsAsync(tenantId, assignment.SchoolClassId, ct))
                throw new InvalidOperationException("One or more school classes are invalid");

            if (assignment.SectionId.HasValue &&
                !await repo.SectionBelongsToSchoolClassAsync(
                    tenantId, assignment.SchoolClassId, assignment.SectionId.Value, ct))
                throw new InvalidOperationException("One or more sections do not belong to the specified school class");

            if (!await repo.SubjectExistsAsync(tenantId, assignment.SubjectId, ct))
                throw new InvalidOperationException("One or more subjects are invalid");
        }

        await repo.ReplaceAssignmentsAsync(tenantId, teacher.Id, request.Assignments, ct);
        await audit.LogAsync(
            "Update",
            "TeacherAssignment",
            teacher.Id,
            new { request.Assignments },
            ct);

        var updated = await repo.GetByIdAsync(tenantId, teacher.Id, ct)
            ?? throw new InvalidOperationException("Failed to load updated teacher");

        return TeacherMapper.ToDto(updated);
    }
}
