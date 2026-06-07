using FluentValidation;
using MediatR;
using SchoolCom.Application.Common.Interfaces;

namespace SchoolCom.Application.Features.Subjects.Commands.DeleteSubject;

public record DeleteSubjectCommand(Guid Id) : IRequest;

public class DeleteSubjectCommandValidator : AbstractValidator<DeleteSubjectCommand>
{
    public DeleteSubjectCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
    }
}

public class DeleteSubjectCommandHandler(
    ISubjectRepository repo,
    ICurrentUserService currentUser,
    IAuditService audit) : IRequestHandler<DeleteSubjectCommand>
{
    public async Task Handle(DeleteSubjectCommand request, CancellationToken cancellationToken)
    {
        var tenantId = currentUser.TenantId
            ?? throw new UnauthorizedAccessException("Tenant not found");

        var entity = await repo.GetByIdForUpdateAsync(tenantId, request.Id, cancellationToken)
            ?? throw new KeyNotFoundException("Subject not found");

        if (await repo.HasLinkedAssignmentsOrTimetableAsync(tenantId, request.Id, cancellationToken))
            throw new InvalidOperationException("Cannot delete a subject that has linked teacher assignments or timetable entries. Deactivate it instead.");

        await repo.DeleteAsync(entity, cancellationToken);
        await repo.SaveChangesAsync(cancellationToken);
        await audit.LogAsync("Delete", "Subject", entity.Id, ct: cancellationToken);
    }
}
