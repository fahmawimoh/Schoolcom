using SchoolCom.Domain.Entities;

namespace SchoolCom.Application.Features.Timetables;

public interface ITimetableRepository
{
    Task<TimetableEntryListResult> GetPagedAsync(
        Guid tenantId,
        int page,
        int pageSize,
        Guid? schoolClassId,
        Guid? sectionId,
        Guid? teacherId,
        Guid? subjectId,
        string? sortBy,
        bool sortDesc,
        CancellationToken ct);

    Task<TimetableEntry?> GetByIdAsync(Guid tenantId, Guid id, CancellationToken ct);

    Task<TimetableEntry?> GetByIdForUpdateAsync(Guid tenantId, Guid id, CancellationToken ct);

    Task<bool> SchoolClassExistsAsync(Guid tenantId, Guid schoolClassId, CancellationToken ct);

    Task<bool> SectionBelongsToSchoolClassAsync(
        Guid tenantId,
        Guid schoolClassId,
        Guid sectionId,
        CancellationToken ct);

    Task<bool> TeacherExistsAsync(Guid tenantId, Guid teacherId, CancellationToken ct);

    Task<bool> SubjectExistsAsync(Guid tenantId, Guid subjectId, CancellationToken ct);

    Task<bool> RoomExistsAsync(Guid tenantId, Guid roomId, CancellationToken ct);

    Task<bool> SlotExistsAsync(
        Guid tenantId,
        Guid sectionId,
        int dayOfWeek,
        int periodNumber,
        Guid? excludeId,
        CancellationToken ct);

    Task AddAsync(TimetableEntry entity, CancellationToken ct);

    Task DeleteAsync(TimetableEntry entity, CancellationToken ct);

    Task SaveChangesAsync(CancellationToken ct);
}
