using FluentValidation;
using MediatR;
using SchoolCom.Application.Common.Interfaces;

namespace SchoolCom.Application.Features.Timetables.Commands.DeleteTimetableEntry;

public record DeleteTimetableEntryCommand(Guid Id) : IRequest;

public class DeleteTimetableEntryCommandValidator : AbstractValidator<DeleteTimetableEntryCommand>
{
    public DeleteTimetableEntryCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
    }
}

public class DeleteTimetableEntryCommandHandler(
    ITimetableRepository repo,
    ICurrentUserService currentUser,
    IAuditService audit) : IRequestHandler<DeleteTimetableEntryCommand>
{
    public async Task Handle(DeleteTimetableEntryCommand request, CancellationToken cancellationToken)
    {
        var tenantId = currentUser.TenantId
            ?? throw new UnauthorizedAccessException("Tenant not found");

        var entity = await repo.GetByIdForUpdateAsync(tenantId, request.Id, cancellationToken)
            ?? throw new KeyNotFoundException("Timetable entry not found");

        await repo.DeleteAsync(entity, cancellationToken);
        await repo.SaveChangesAsync(cancellationToken);
        await audit.LogAsync("Delete", "TimetableEntry", entity.Id, ct: cancellationToken);
    }
}
