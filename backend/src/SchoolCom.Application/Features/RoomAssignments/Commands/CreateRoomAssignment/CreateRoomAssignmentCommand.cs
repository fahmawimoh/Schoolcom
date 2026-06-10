using FluentValidation;
using MediatR;
using SchoolCom.Application.Common;
using SchoolCom.Application.Common.Exceptions;
using SchoolCom.Application.Common.Interfaces;
using SchoolCom.Domain.Entities;

namespace SchoolCom.Application.Features.RoomAssignments.Commands.CreateRoomAssignment;

public record CreateRoomAssignmentCommand(Guid ClassSectionId, Guid RoomId) : IRequest<RoomAssignmentDto>;

public class CreateRoomAssignmentCommandValidator : AbstractValidator<CreateRoomAssignmentCommand>
{
    public CreateRoomAssignmentCommandValidator()
    {
        RuleFor(x => x.ClassSectionId).NotEmpty();
        RuleFor(x => x.RoomId).NotEmpty();
    }
}

public class CreateRoomAssignmentCommandHandler(
    IRoomAssignmentRepository repo,
    ICurrentUserService currentUser,
    IAuditService audit) : IRequestHandler<CreateRoomAssignmentCommand, RoomAssignmentDto>
{
    public async Task<RoomAssignmentDto> Handle(CreateRoomAssignmentCommand request, CancellationToken ct)
    {
        var tenantId = currentUser.TenantId
            ?? throw new UnauthorizedAccessException("Tenant not found");

        if (!await repo.ClassSectionExistsAsync(tenantId, request.ClassSectionId, ct))
            throw new KeyNotFoundException("Class-section not found");

        if (!await repo.RoomExistsAsync(tenantId, request.RoomId, ct))
            throw new KeyNotFoundException("Room not found");

        if (await repo.ClassSectionAlreadyAssignedAsync(tenantId, request.ClassSectionId, excludeId: null, ct) ||
            await repo.RoomAlreadyAssignedAsync(tenantId, request.RoomId, excludeId: null, ct))
        {
            throw new ConflictException(
                AcademicErrorCodes.RoomAssignmentDuplicate,
                AcademicErrorMessages.RoomAssignmentDuplicateEn,
                AcademicErrorMessages.RoomAssignmentDuplicateAr,
                AcademicTranslationKeys.RoomAssignmentDuplicate);
        }

        var entity = new RoomAssignment
        {
            TenantId = tenantId,
            ClassSectionId = request.ClassSectionId,
            RoomId = request.RoomId
        };

        await repo.AddAsync(entity, ct);
        await repo.SaveChangesAsync(ct);
        await audit.LogAsync("Create", "RoomAssignment", entity.Id, ct: ct);

        var created = await repo.GetByIdAsync(tenantId, entity.Id, ct)
            ?? throw new InvalidOperationException("Failed to load created room assignment");

        return Map(created);
    }

    private static RoomAssignmentDto Map(RoomAssignment x) =>
        new(
            x.Id,
            x.ClassSectionId,
            x.ClassSection.SchoolClassId,
            x.ClassSection.SchoolClass.NameEn,
            x.ClassSection.SchoolClass.NameAr,
            x.ClassSection.SectionId,
            x.ClassSection.Section.NameEn,
            x.ClassSection.Section.NameAr,
            x.RoomId,
            x.Room.NameEn,
            x.Room.NameAr,
            x.Room.Code,
            x.CreatedAt);
}
