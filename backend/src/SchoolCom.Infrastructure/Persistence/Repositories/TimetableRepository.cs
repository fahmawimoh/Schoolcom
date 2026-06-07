using Microsoft.EntityFrameworkCore;
using SchoolCom.Application.Common;
using SchoolCom.Application.Features.Timetables;
using SchoolCom.Domain.Entities;

namespace SchoolCom.Infrastructure.Persistence.Repositories;

public class TimetableRepository(ApplicationDbContext db) : ITimetableRepository
{
    private IQueryable<TimetableEntry> BaseQuery(Guid tenantId) =>
        db.TimetableEntries.AsNoTracking().Where(x => x.TenantId == tenantId);

    public async Task<TimetableEntryListResult> GetPagedAsync(
        Guid tenantId,
        int page,
        int pageSize,
        Guid? schoolClassId,
        Guid? sectionId,
        Guid? teacherId,
        Guid? subjectId,
        string? sortBy,
        bool sortDesc,
        CancellationToken ct)
    {
        var query = BaseQuery(tenantId);

        if (schoolClassId.HasValue)
            query = query.Where(x => x.SchoolClassId == schoolClassId.Value);

        if (sectionId.HasValue)
            query = query.Where(x => x.SectionId == sectionId.Value);

        if (teacherId.HasValue)
            query = query.Where(x => x.TeacherId == teacherId.Value);

        if (subjectId.HasValue)
            query = query.Where(x => x.SubjectId == subjectId.Value);

        query = (sortBy?.ToLower()) switch
        {
            "dayofweek" => sortDesc
                ? query.OrderByDescending(x => x.DayOfWeek).ThenByDescending(x => x.PeriodNumber)
                : query.OrderBy(x => x.DayOfWeek).ThenBy(x => x.PeriodNumber),
            "periodnumber" => sortDesc
                ? query.OrderByDescending(x => x.PeriodNumber)
                : query.OrderBy(x => x.PeriodNumber),
            "starttime" => sortDesc
                ? query.OrderByDescending(x => x.StartTime)
                : query.OrderBy(x => x.StartTime),
            "createdat" => sortDesc
                ? query.OrderByDescending(x => x.CreatedAt)
                : query.OrderBy(x => x.CreatedAt),
            _ => sortDesc
                ? query.OrderByDescending(x => x.DayOfWeek).ThenByDescending(x => x.PeriodNumber)
                : query.OrderBy(x => x.DayOfWeek).ThenBy(x => x.PeriodNumber)
        };

        var totalCount = await query.CountAsync(ct);
        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(x => new TimetableEntryDto(
                x.Id,
                x.SchoolClassId,
                x.SchoolClass.NameEn,
                x.SchoolClass.NameAr,
                x.SectionId,
                x.Section.NameEn,
                x.Section.NameAr,
                x.TeacherId,
                x.Teacher.FirstNameEn,
                x.Teacher.FirstNameAr,
                x.Teacher.LastNameEn,
                x.Teacher.LastNameAr,
                x.SubjectId,
                x.Subject.NameEn,
                x.Subject.NameAr,
                x.RoomId,
                x.Room.NameEn,
                x.Room.NameAr,
                x.DayOfWeek,
                x.PeriodNumber,
                x.StartTime,
                x.EndTime,
                x.CreatedAt))
            .ToListAsync(ct);

        return new TimetableEntryListResult(
            items,
            totalCount,
            page,
            pageSize,
            PaginationHelper.TotalPages(totalCount, pageSize));
    }

    public Task<TimetableEntry?> GetByIdAsync(Guid tenantId, Guid id, CancellationToken ct) =>
        db.TimetableEntries.AsNoTracking()
            .Include(x => x.SchoolClass)
            .Include(x => x.Section)
            .Include(x => x.Teacher)
            .Include(x => x.Subject)
            .Include(x => x.Room)
            .FirstOrDefaultAsync(x => x.TenantId == tenantId && x.Id == id, ct);

    public Task<TimetableEntry?> GetByIdForUpdateAsync(Guid tenantId, Guid id, CancellationToken ct) =>
        db.TimetableEntries.FirstOrDefaultAsync(x => x.TenantId == tenantId && x.Id == id, ct);

    public Task<bool> SchoolClassExistsAsync(Guid tenantId, Guid schoolClassId, CancellationToken ct) =>
        db.SchoolClasses.AnyAsync(x => x.TenantId == tenantId && x.Id == schoolClassId, ct);

    public Task<bool> SectionBelongsToSchoolClassAsync(
        Guid tenantId,
        Guid schoolClassId,
        Guid sectionId,
        CancellationToken ct) =>
        db.Sections.AnyAsync(
            x => x.TenantId == tenantId && x.Id == sectionId && x.SchoolClassId == schoolClassId,
            ct);

    public Task<bool> TeacherExistsAsync(Guid tenantId, Guid teacherId, CancellationToken ct) =>
        db.Teachers.AnyAsync(x => x.TenantId == tenantId && x.Id == teacherId, ct);

    public Task<bool> SubjectExistsAsync(Guid tenantId, Guid subjectId, CancellationToken ct) =>
        db.Subjects.AnyAsync(x => x.TenantId == tenantId && x.Id == subjectId, ct);

    public Task<bool> RoomExistsAsync(Guid tenantId, Guid roomId, CancellationToken ct) =>
        db.Rooms.AnyAsync(x => x.TenantId == tenantId && x.Id == roomId, ct);

    public Task<bool> SlotExistsAsync(
        Guid tenantId,
        Guid sectionId,
        int dayOfWeek,
        int periodNumber,
        Guid? excludeId,
        CancellationToken ct) =>
        db.TimetableEntries.AnyAsync(
            x => x.TenantId == tenantId &&
                 x.SectionId == sectionId &&
                 x.DayOfWeek == dayOfWeek &&
                 x.PeriodNumber == periodNumber &&
                 (!excludeId.HasValue || x.Id != excludeId.Value),
            ct);

    public async Task AddAsync(TimetableEntry entity, CancellationToken ct) =>
        await db.TimetableEntries.AddAsync(entity, ct);

    public Task DeleteAsync(TimetableEntry entity, CancellationToken ct)
    {
        db.TimetableEntries.Remove(entity);
        return Task.CompletedTask;
    }

    public Task SaveChangesAsync(CancellationToken ct) => db.SaveChangesAsync(ct);
}
