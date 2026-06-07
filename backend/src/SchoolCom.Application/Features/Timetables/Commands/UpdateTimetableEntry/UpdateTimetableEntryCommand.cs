using FluentValidation;
using MediatR;
using SchoolCom.Application.Common.Interfaces;
using SchoolCom.Application.Features.Timetables.Commands.CreateTimetableEntry;

namespace SchoolCom.Application.Features.Timetables.Commands.UpdateTimetableEntry;

public record UpdateTimetableEntryCommand(
    Guid Id,
    Guid SchoolClassId,
    Guid SectionId,
    Guid TeacherId,
    Guid SubjectId,
    Guid RoomId,
    int DayOfWeek,
    int PeriodNumber,
    TimeOnly StartTime,
    TimeOnly EndTime) : IRequest<TimetableEntryDto>;

public class UpdateTimetableEntryCommandValidator : AbstractValidator<UpdateTimetableEntryCommand>
{
    public UpdateTimetableEntryCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
        RuleFor(x => x.SchoolClassId).NotEmpty();
        RuleFor(x => x.SectionId).NotEmpty();
        RuleFor(x => x.TeacherId).NotEmpty();
        RuleFor(x => x.SubjectId).NotEmpty();
        RuleFor(x => x.RoomId).NotEmpty();
        RuleFor(x => x.DayOfWeek).InclusiveBetween(1, 7);
        RuleFor(x => x.PeriodNumber).GreaterThan(0);
        RuleFor(x => x.EndTime).GreaterThan(x => x.StartTime);
    }
}

public class UpdateTimetableEntryCommandHandler(
    ITimetableRepository repo,
    ICurrentUserService currentUser,
    IAuditService audit) : IRequestHandler<UpdateTimetableEntryCommand, TimetableEntryDto>
{
    public async Task<TimetableEntryDto> Handle(UpdateTimetableEntryCommand request, CancellationToken ct)
    {
        var tenantId = currentUser.TenantId
            ?? throw new UnauthorizedAccessException("Tenant not found");

        var entity = await repo.GetByIdForUpdateAsync(tenantId, request.Id, ct)
            ?? throw new KeyNotFoundException("Timetable entry not found");

        await CreateTimetableEntryCommandHandler.ValidateReferencesAsync(
            repo,
            tenantId,
            new CreateTimetableEntryCommand(
                request.SchoolClassId,
                request.SectionId,
                request.TeacherId,
                request.SubjectId,
                request.RoomId,
                request.DayOfWeek,
                request.PeriodNumber,
                request.StartTime,
                request.EndTime),
            ct);

        if (await repo.SlotExistsAsync(
                tenantId, request.SectionId, request.DayOfWeek, request.PeriodNumber, request.Id, ct))
            throw new InvalidOperationException("A timetable entry already exists for this section, day, and period");

        entity.SchoolClassId = request.SchoolClassId;
        entity.SectionId = request.SectionId;
        entity.TeacherId = request.TeacherId;
        entity.SubjectId = request.SubjectId;
        entity.RoomId = request.RoomId;
        entity.DayOfWeek = request.DayOfWeek;
        entity.PeriodNumber = request.PeriodNumber;
        entity.StartTime = request.StartTime;
        entity.EndTime = request.EndTime;

        await repo.SaveChangesAsync(ct);
        await audit.LogAsync("Update", "TimetableEntry", entity.Id, ct: ct);

        var updated = await repo.GetByIdAsync(tenantId, entity.Id, ct)
            ?? throw new InvalidOperationException("Failed to load updated timetable entry");

        return TimetableMapper.ToDto(updated);
    }
}
