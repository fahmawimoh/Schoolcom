using FluentValidation;
using MediatR;
using SchoolCom.Application.Common;
using SchoolCom.Application.Common.Exceptions;
using SchoolCom.Application.Common.Interfaces;

namespace SchoolCom.Application.Features.RoomAssignments.Commands.UpdateRoomAssignment;

public record UpdateRoomAssignmentCommand(Guid Id, Guid ClassSectionId, Guid RoomId) : IRequest<RoomAssignmentDto>;

public class UpdateRoomAssignmentCommandValidator : AbstractValidator<UpdateRoomAssignmentCommand>
{
    public UpdateRoomAssignmentCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
        RuleFor(x => x.ClassSectionId).NotEmpty();
        RuleFor(x => x.RoomId).NotEmpty();
    }
}

public class UpdateRoomAssignmentCommandHandler(
    IRoomAssignmentRepository repo,
    ICurrentUserService currentUser,
    IAuditService audit) : IRequestHandler<UpdateRoomAssignmentCommand, RoomAssignmentDto>
{
    public async Task<RoomAssignmentDto> Handle(UpdateRoomAssignmentCommand request, CancellationToken ct)
    {
        var tenantId = currentUser.TenantId
            ?? throw new UnauthorizedAccessException("Tenant not found");

        var entity = await repo.GetByIdForUpdateAsync(tenantId, request.Id, ct)
            ?? throw new KeyNotFoundException("Room assignment not found");

        if (!await repo.ClassSectionExistsAsync(tenantId, request.ClassSectionId, ct))
            throw new KeyNotFoundException("Class-section not found");

        if (!await repo.RoomExistsAsync(tenantId, request.RoomId, ct))
            throw new KeyNotFoundException("Room not found");

        if (await repo.ClassSectionAlreadyAssignedAsync(tenantId, request.ClassSectionId, excludeId: entity.Id, ct) ||
            await repo.RoomAlreadyAssignedAsync(tenantId, request.RoomId, excludeId: entity.Id, ct))
        {
            throw new ConflictException(
                AcademicErrorCodes.RoomAssignmentDuplicate,
                AcademicErrorMessages.RoomAssignmentDuplicateEn,
                AcademicErrorMessages.RoomAssignmentDuplicateAr,
                AcademicTranslationKeys.RoomAssignmentDuplicate);
        }

        entity.ClassSectionId = request.ClassSectionId;
        entity.RoomId = request.RoomId;

        await repo.SaveChangesAsync(ct);
        await audit.LogAsync("Update", "RoomAssignment", entity.Id, ct: ct);

        var updated = await repo.GetByIdAsync(tenantId, entity.Id, ct)
            ?? throw new InvalidOperationException("Failed to load updated room assignment");

        return new RoomAssignmentDto(
            updated.Id,
            updated.ClassSectionId,
            updated.ClassSection.SchoolClassId,
            updated.ClassSection.SchoolClass.NameEn,
            updated.ClassSection.SchoolClass.NameAr,
            updated.ClassSection.SectionId,
            updated.ClassSection.Section.NameEn,
            updated.ClassSection.Section.NameAr,
            updated.RoomId,
            updated.Room.NameEn,
            updated.Room.NameAr,
            updated.Room.Code,
            updated.CreatedAt);
    }
}
