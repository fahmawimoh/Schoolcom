using FluentValidation;
using MediatR;
using SchoolCom.Application.Common.Interfaces;

namespace SchoolCom.Application.Features.RoomAssignments.Commands.DeleteRoomAssignment;

public record DeleteRoomAssignmentCommand(Guid Id) : IRequest;

public class DeleteRoomAssignmentCommandValidator : AbstractValidator<DeleteRoomAssignmentCommand>
{
    public DeleteRoomAssignmentCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
    }
}

public class DeleteRoomAssignmentCommandHandler(
    IRoomAssignmentRepository repo,
    ICurrentUserService currentUser,
    IAuditService audit) : IRequestHandler<DeleteRoomAssignmentCommand>
{
    public async Task Handle(DeleteRoomAssignmentCommand request, CancellationToken cancellationToken)
    {
        var tenantId = currentUser.TenantId
            ?? throw new UnauthorizedAccessException("Tenant not found");

        var entity = await repo.GetByIdForUpdateAsync(tenantId, request.Id, cancellationToken)
            ?? throw new KeyNotFoundException("Room assignment not found");

        await repo.DeleteAsync(entity, cancellationToken);
        await repo.SaveChangesAsync(cancellationToken);
        await audit.LogAsync("Delete", "RoomAssignment", entity.Id, ct: cancellationToken);
    }
}
