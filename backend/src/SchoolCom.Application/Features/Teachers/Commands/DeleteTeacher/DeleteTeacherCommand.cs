using FluentValidation;
using MediatR;
using SchoolCom.Application.Common.Interfaces;

namespace SchoolCom.Application.Features.Teachers.Commands.DeleteTeacher;

public record DeleteTeacherCommand(Guid Id) : IRequest;

public class DeleteTeacherCommandValidator : AbstractValidator<DeleteTeacherCommand>
{
    public DeleteTeacherCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
    }
}

public class DeleteTeacherCommandHandler(
    ITeacherRepository repo,
    ICurrentUserService currentUser,
    IAuditService audit) : IRequestHandler<DeleteTeacherCommand>
{
    public async Task Handle(DeleteTeacherCommand request, CancellationToken cancellationToken)
    {
        var tenantId = currentUser.TenantId
            ?? throw new UnauthorizedAccessException("Tenant not found");

        var entity = await repo.GetByIdForUpdateAsync(tenantId, request.Id, cancellationToken)
            ?? throw new KeyNotFoundException("Teacher not found");

        if (await repo.HasLinkedTimetableEntriesAsync(tenantId, request.Id, cancellationToken))
            throw new InvalidOperationException(
                "Cannot delete a teacher that has linked timetable entries. Deactivate the teacher instead.");

        await repo.DeleteAsync(entity, cancellationToken);
        await repo.SaveChangesAsync(cancellationToken);
        await audit.LogAsync("Delete", "Teacher", entity.Id, ct: cancellationToken);
    }
}
