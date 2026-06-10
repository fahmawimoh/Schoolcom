using FluentValidation;
using MediatR;
using SchoolCom.Application.Common.Interfaces;
using SchoolCom.Application.Features.RoomAssignments;

namespace SchoolCom.Application.Features.Rooms.Commands.DeleteRoom;

public record DeleteRoomCommand(Guid Id) : IRequest;

public class DeleteRoomCommandValidator : AbstractValidator<DeleteRoomCommand>
{
    public DeleteRoomCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
    }
}

public class DeleteRoomCommandHandler(
    IRoomRepository repo,
    IRoomAssignmentRepository assignmentRepo,
    ICurrentUserService currentUser,
    IAuditService audit) : IRequestHandler<DeleteRoomCommand>
{
    public async Task Handle(DeleteRoomCommand request, CancellationToken cancellationToken)
    {
        var tenantId = currentUser.TenantId
            ?? throw new UnauthorizedAccessException("Tenant not found");

        var entity = await repo.GetByIdForUpdateAsync(tenantId, request.Id, cancellationToken)
            ?? throw new KeyNotFoundException("Room not found");

        if (await repo.HasLinkedTimetableEntriesAsync(tenantId, request.Id, cancellationToken))
            throw new InvalidOperationException("Cannot delete a room that has linked timetable entries. Deactivate it instead.");

        var assignment = await assignmentRepo.GetByRoomIdForUpdateAsync(
            tenantId, request.Id, cancellationToken);
        if (assignment is not null)
            await assignmentRepo.DeleteAsync(assignment, cancellationToken);

        await repo.DeleteAsync(entity, cancellationToken);
        await repo.SaveChangesAsync(cancellationToken);
        await audit.LogAsync("Delete", "Room", entity.Id, ct: cancellationToken);
    }
}
