using FluentValidation;
using MediatR;
using SchoolCom.Application.Common.Interfaces;
using SchoolCom.Domain.Entities;

namespace SchoolCom.Application.Features.Timetables.Commands.CreateTimetableEntry;

public record CreateTimetableEntryCommand(
    Guid SchoolClassId,
    Guid SectionId,
    Guid TeacherId,
    Guid SubjectId,
    Guid RoomId,
    int DayOfWeek,
    int PeriodNumber,
    TimeOnly StartTime,
    TimeOnly EndTime) : IRequest<TimetableEntryDto>;

public class CreateTimetableEntryCommandValidator : AbstractValidator<CreateTimetableEntryCommand>
{
    public CreateTimetableEntryCommandValidator()
    {
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

public class CreateTimetableEntryCommandHandler(
    ITimetableRepository repo,
    ICurrentUserService currentUser,
    IAuditService audit) : IRequestHandler<CreateTimetableEntryCommand, TimetableEntryDto>
{
    public async Task<TimetableEntryDto> Handle(CreateTimetableEntryCommand request, CancellationToken ct)
    {
        var tenantId = currentUser.TenantId
            ?? throw new UnauthorizedAccessException("Tenant not found");

        await ValidateReferencesAsync(repo, tenantId, request, ct);

        if (await repo.SlotExistsAsync(
                tenantId, request.SectionId, request.DayOfWeek, request.PeriodNumber, null, ct))
            throw new InvalidOperationException("A timetable entry already exists for this section, day, and period");

        var entity = new TimetableEntry
        {
            TenantId = tenantId,
            SchoolClassId = request.SchoolClassId,
            SectionId = request.SectionId,
            TeacherId = request.TeacherId,
            SubjectId = request.SubjectId,
            RoomId = request.RoomId,
            DayOfWeek = request.DayOfWeek,
            PeriodNumber = request.PeriodNumber,
            StartTime = request.StartTime,
            EndTime = request.EndTime
        };

        await repo.AddAsync(entity, ct);
        await repo.SaveChangesAsync(ct);
        await audit.LogAsync("Create", "TimetableEntry", entity.Id, ct: ct);

        var created = await repo.GetByIdAsync(tenantId, entity.Id, ct)
            ?? throw new InvalidOperationException("Failed to load created timetable entry");

        return TimetableMapper.ToDto(created);
    }

    internal static async Task ValidateReferencesAsync(
        ITimetableRepository repo,
        Guid tenantId,
        CreateTimetableEntryCommand request,
        CancellationToken ct)
    {
        if (!await repo.SchoolClassExistsAsync(tenantId, request.SchoolClassId, ct))
            throw new KeyNotFoundException("School class not found");

        if (!await repo.SectionBelongsToSchoolClassAsync(
                tenantId, request.SchoolClassId, request.SectionId, ct))
            throw new InvalidOperationException("Section does not belong to the specified school class");

        if (!await repo.TeacherExistsAsync(tenantId, request.TeacherId, ct))
            throw new KeyNotFoundException("Teacher not found");

        if (!await repo.SubjectExistsAsync(tenantId, request.SubjectId, ct))
            throw new KeyNotFoundException("Subject not found");

        if (!await repo.RoomExistsAsync(tenantId, request.RoomId, ct))
            throw new KeyNotFoundException("Room not found");
    }
}
