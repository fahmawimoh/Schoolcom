using FluentValidation;
using MediatR;
using SchoolCom.Application.Common;
using SchoolCom.Application.Common.Exceptions;
using SchoolCom.Application.Common.Interfaces;
using SchoolCom.Application.Features.ClassSections;
using SchoolCom.Application.Features.RoomAssignments;
using SchoolCom.Domain.Entities;

namespace SchoolCom.Application.Features.Rooms.Commands.CreateRoom;

public record CreateRoomCommand(
    string NameEn,
    string NameAr,
    int? Capacity,
    bool IsActive,
    Guid SchoolClassId,
    Guid SectionId) : IRequest<RoomDto>;

public class CreateRoomCommandValidator : AbstractValidator<CreateRoomCommand>
{
    public CreateRoomCommandValidator()
    {
        RuleFor(x => x.NameEn).NotEmpty().MaximumLength(100);
        RuleFor(x => x.NameAr).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Capacity).GreaterThan(0).When(x => x.Capacity.HasValue);
        RuleFor(x => x.SchoolClassId).NotEmpty();
        RuleFor(x => x.SectionId).NotEmpty();
    }
}

public class CreateRoomCommandHandler(
    IRoomRepository roomRepo,
    IClassSectionRepository classSectionRepo,
    IRoomAssignmentRepository assignmentRepo,
    ICurrentUserService currentUser,
    IAuditService audit) : IRequestHandler<CreateRoomCommand, RoomDto>
{
    public async Task<RoomDto> Handle(CreateRoomCommand request, CancellationToken ct)
    {
        var tenantId = currentUser.TenantId
            ?? throw new UnauthorizedAccessException("Tenant not found");

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

        if (await assignmentRepo.ClassSectionAlreadyAssignedAsync(
                tenantId, classSection.Id, excludeId: null, ct))
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
                excludeRoomId: null,
                ct))
        {
            throw new ConflictException(
                AcademicErrorCodes.RoomAssignmentDuplicate,
                AcademicErrorMessages.RoomAssignmentDuplicateEn,
                AcademicErrorMessages.RoomAssignmentDuplicateAr,
                AcademicTranslationKeys.RoomAssignmentDuplicate);
        }

        var roomCode = await roomRepo.GenerateUniqueRoomCodeAsync(tenantId, ct);

        var entity = new Room
        {
            TenantId = tenantId,
            NameEn = request.NameEn.Trim(),
            NameAr = request.NameAr.Trim(),
            Code = roomCode,
            Capacity = request.Capacity,
            IsActive = request.IsActive
        };

        await roomRepo.AddAsync(entity, ct);
        await roomRepo.SaveChangesAsync(ct);

        await assignmentRepo.AddAsync(new RoomAssignment
        {
            TenantId = tenantId,
            ClassSectionId = classSection.Id,
            RoomId = entity.Id
        }, ct);
        await assignmentRepo.SaveChangesAsync(ct);

        await audit.LogAsync("Create", "Room", entity.Id, ct: ct);

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
