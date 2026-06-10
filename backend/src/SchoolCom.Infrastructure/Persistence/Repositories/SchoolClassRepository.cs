using Microsoft.EntityFrameworkCore;
using SchoolCom.Application.Common;
using SchoolCom.Application.Features.SchoolClasses;
using SchoolCom.Domain.Entities;

namespace SchoolCom.Infrastructure.Persistence.Repositories;

public class SchoolClassRepository(ApplicationDbContext db) : ISchoolClassRepository
{
    public async Task<SchoolClassListResult> GetPagedAsync(
        Guid tenantId,
        int page,
        int pageSize,
        string? search,
        bool? isActive,
        string? sortBy,
        bool sortDesc,
        CancellationToken ct)
    {
        var query = db.SchoolClasses.AsNoTracking().Where(x => x.TenantId == tenantId);

        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim().ToLower();
            query = query.Where(x =>
                x.NameEn.ToLower().Contains(term) ||
                x.NameAr.Contains(term) ||
                (x.DescriptionEn != null && x.DescriptionEn.ToLower().Contains(term)) ||
                (x.DescriptionAr != null && x.DescriptionAr.Contains(term)));
        }

        if (isActive.HasValue)
            query = query.Where(x => x.IsActive == isActive.Value);

        query = (sortBy?.ToLower()) switch
        {
            "nameen" => sortDesc ? query.OrderByDescending(x => x.NameEn) : query.OrderBy(x => x.NameEn),
            "namear" => sortDesc ? query.OrderByDescending(x => x.NameAr) : query.OrderBy(x => x.NameAr),
            "createdat" => sortDesc ? query.OrderByDescending(x => x.CreatedAt) : query.OrderBy(x => x.CreatedAt),
            _ => sortDesc ? query.OrderByDescending(x => x.SortOrder) : query.OrderBy(x => x.SortOrder)
        };

        var totalCount = await query.CountAsync(ct);
        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(x => new SchoolClassDto(
                x.Id,
                x.NameEn,
                x.NameAr,
                x.DescriptionEn,
                x.DescriptionAr,
                x.SortOrder,
                x.IsActive,
                x.ClassSections.Count,
                x.ClassSections
                    .OrderBy(cs => cs.Section.NameEn)
                    .Select(cs => new SchoolClassSectionItem(
                        cs.SectionId,
                        cs.Section.NameEn,
                        cs.Section.NameAr))
                    .ToList(),
                x.Students.Count,
                x.CreatedAt))
            .ToListAsync(ct);

        return new SchoolClassListResult(
            items,
            totalCount,
            page,
            pageSize,
            PaginationHelper.TotalPages(totalCount, pageSize));
    }

    public Task<SchoolClass?> GetByIdAsync(Guid tenantId, Guid id, CancellationToken ct) =>
        db.SchoolClasses.AsNoTracking()
            .Include(x => x.ClassSections).ThenInclude(cs => cs.Section)
            .Include(x => x.Students)
            .FirstOrDefaultAsync(x => x.TenantId == tenantId && x.Id == id, ct);

    public Task<SchoolClass?> GetByIdForUpdateAsync(Guid tenantId, Guid id, CancellationToken ct) =>
        db.SchoolClasses.FirstOrDefaultAsync(x => x.TenantId == tenantId && x.Id == id, ct);

    public Task<bool> NameExistsAsync(Guid tenantId, string nameEn, Guid? excludeId, CancellationToken ct)
    {
        var normalized = nameEn.Trim().ToLower();
        var query = db.SchoolClasses.Where(x =>
            x.TenantId == tenantId &&
            x.NameEn.ToLower() == normalized);

        if (excludeId.HasValue)
            query = query.Where(x => x.Id != excludeId.Value);

        return query.AnyAsync(ct);
    }

    public async Task<bool> HasLinkedStudentsOrAssignmentsAsync(Guid tenantId, Guid id, CancellationToken ct)
    {
        if (await db.Students.AnyAsync(s => s.TenantId == tenantId && s.SchoolClassId == id, ct))
            return true;

        if (await db.TeacherAssignments.AnyAsync(ta => ta.SchoolClassId == id, ct))
            return true;

        if (await db.TimetableEntries.AnyAsync(te => te.TenantId == tenantId && te.SchoolClassId == id, ct))
            return true;

        var classSectionIds = await db.ClassSections
            .Where(cs => cs.TenantId == tenantId && cs.SchoolClassId == id)
            .Select(cs => cs.Id)
            .ToListAsync(ct);

        if (classSectionIds.Count == 0)
            return false;

        if (await db.RoomAssignments.AnyAsync(
                ra => ra.TenantId == tenantId && classSectionIds.Contains(ra.ClassSectionId), ct))
            return true;

        if (await db.TeacherAssignments.AnyAsync(
                ta => ta.ClassSectionId.HasValue && classSectionIds.Contains(ta.ClassSectionId.Value), ct))
            return true;

        return await db.TimetableEntries.AnyAsync(
            te => te.TenantId == tenantId &&
                  te.ClassSectionId.HasValue &&
                  classSectionIds.Contains(te.ClassSectionId.Value),
            ct);
    }

    public async Task AddAsync(SchoolClass entity, CancellationToken ct) =>
        await db.SchoolClasses.AddAsync(entity, ct);

    public Task DeleteAsync(SchoolClass entity, CancellationToken ct)
    {
        db.SchoolClasses.Remove(entity);
        return Task.CompletedTask;
    }

    public Task SaveChangesAsync(CancellationToken ct) => db.SaveChangesAsync(ct);
}
