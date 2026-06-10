using FluentValidation;
using MediatR;
using SchoolCom.Application.Common;
using SchoolCom.Application.Common.Exceptions;
using SchoolCom.Application.Common.Interfaces;
using SchoolCom.Application.Features.ClassSections;
using SchoolCom.Application.Features.RoomAssignments;
using SchoolCom.Domain.Entities;

namespace SchoolCom.Application.Features.Rooms.Commands.UpdateRoom;

public record UpdateRoomCommand(
    Guid Id,
    string NameEn,
    string NameAr,
    int? Capacity,
    bool IsActive,
    Guid SchoolClassId,
    Guid SectionId) : IRequest<RoomDto>;

public class UpdateRoomCommandValidator : AbstractValidator<UpdateRoomCommand>
{
    public UpdateRoomCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
        RuleFor(x => x.NameEn).NotEmpty().MaximumLength(100);
        RuleFor(x => x.NameAr).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Capacity).GreaterThan(0).When(x => x.Capacity.HasValue);
        RuleFor(x => x.SchoolClassId).NotEmpty();
        RuleFor(x => x.SectionId).NotEmpty();
    }
}

public class UpdateRoomCommandHandler(
    IRoomRepository roomRepo,
    IClassSectionRepository classSectionRepo,
    IRoomAssignmentRepository assignmentRepo,
    ICurrentUserService currentUser,
    IAuditService audit) : IRequestHandler<UpdateRoomCommand, RoomDto>
{
    public async Task<RoomDto> Handle(UpdateRoomCommand request, CancellationToken ct)
    {
        var tenantId = currentUser.TenantId
            ?? throw new UnauthorizedAccessException("Tenant not found");

        var entity = await roomRepo.GetByIdForUpdateAsync(tenantId, request.Id, ct)
            ?? throw new KeyNotFoundException("Room not found");

        var classSection = await classSectionRepo.GetByClassAndSectionAsync(
            tenantId, request.SchoolClassId, request.SectionId, ct);

        if (classSection is null)
        {
            throw new ConflictException(
                AcademicErrorCodes.ClassSectionNotFound,
                AcademicErrorMessages.ClassSectionNotFoundEn,
                AcademicErrorMessages.ClassSectionNotFoundAr,
                AcademicTranslationKeys.ClassSectionDuplicate);
        }

        var assignment = await assignmentRepo.GetByRoomIdForUpdateAsync(tenantId, entity.Id, ct);

        if (await assignmentRepo.ClassSectionAlreadyAssignedAsync(
                tenantId, classSection.Id, excludeId: assignment?.Id, ct))
        {
            throw new ConflictException(
                AcademicErrorCodes.RoomAssignmentDuplicate,
                AcademicErrorMessages.RoomAssignmentDuplicateEn,
                AcademicErrorMessages.RoomAssignmentDuplicateAr,
                AcademicTranslationKeys.RoomAssignmentDuplicate);
        }

        if (await assignmentRepo.ExistsForClassSectionAndRoomNameAsync(
                tenantId,
                classSection.Id,
                request.NameEn,
                excludeRoomId: entity.Id,
                ct))
        {
            throw new ConflictException(
                AcademicErrorCodes.RoomAssignmentDuplicate,
                AcademicErrorMessages.RoomAssignmentDuplicateEn,
                AcademicErrorMessages.RoomAssignmentDuplicateAr,
                AcademicTranslationKeys.RoomAssignmentDuplicate);
        }

        entity.NameEn = request.NameEn.Trim();
        entity.NameAr = request.NameAr.Trim();
        entity.Capacity = request.Capacity;
        entity.IsActive = request.IsActive;

        if (assignment is null)
        {
            await assignmentRepo.AddAsync(new RoomAssignment
            {
                TenantId = tenantId,
                ClassSectionId = classSection.Id,
                RoomId = entity.Id
            }, ct);
        }
        else
        {
            assignment.ClassSectionId = classSection.Id;
        }

        await roomRepo.SaveChangesAsync(ct);
        await assignmentRepo.SaveChangesAsync(ct);
        await audit.LogAsync("Update", "Room", entity.Id, ct: ct);

        return new RoomDto(
            entity.Id,
            entity.NameEn,
            entity.NameAr,
            entity.Code,
            entity.Capacity,
            entity.IsActive,
            classSection.SchoolClassId,
            classSection.SectionId,
            classSection.Id,
            entity.CreatedAt);
    }
}
