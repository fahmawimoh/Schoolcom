using Microsoft.EntityFrameworkCore;
using SchoolCom.Application.Common;
using SchoolCom.Application.Features.Sections;
using SchoolCom.Domain.Entities;

namespace SchoolCom.Infrastructure.Persistence.Repositories;

public class SectionRepository(ApplicationDbContext db) : ISectionRepository
{
    private IQueryable<Section> BaseQuery(Guid tenantId) =>
        db.Sections.AsNoTracking()
            .Include(x => x.SchoolClass)
            .Where(x => x.TenantId == tenantId);

    public async Task<SectionListResult> GetPagedAsync(
        Guid tenantId,
        int page,
        int pageSize,
        Guid? schoolClassId,
        string? search,
        bool? isActive,
        string? sortBy,
        bool sortDesc,
        CancellationToken ct)
    {
        var query = BaseQuery(tenantId);

        if (schoolClassId.HasValue)
            query = query.Where(x => x.SchoolClassId == schoolClassId.Value);

        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim().ToLower();
            query = query.Where(x =>
                x.NameEn.ToLower().Contains(term) ||
                x.NameAr.Contains(term) ||
                x.SchoolClass.NameEn.ToLower().Contains(term) ||
                x.SchoolClass.NameAr.Contains(term) ||
                (x.DescriptionEn != null && x.DescriptionEn.ToLower().Contains(term)) ||
                (x.DescriptionAr != null && x.DescriptionAr.Contains(term)));
        }

        if (isActive.HasValue)
            query = query.Where(x => x.IsActive == isActive.Value);

        query = (sortBy?.ToLower()) switch
        {
            "nameen" => sortDesc ? query.OrderByDescending(x => x.NameEn) : query.OrderBy(x => x.NameEn),
            "namear" => sortDesc ? query.OrderByDescending(x => x.NameAr) : query.OrderBy(x => x.NameAr),
            "schoolclass" => sortDesc
                ? query.OrderByDescending(x => x.SchoolClass.SortOrder)
                : query.OrderBy(x => x.SchoolClass.SortOrder),
            "createdat" => sortDesc ? query.OrderByDescending(x => x.CreatedAt) : query.OrderBy(x => x.CreatedAt),
            _ => sortDesc
                ? query.OrderByDescending(x => x.SchoolClass.SortOrder).ThenByDescending(x => x.NameEn)
                : query.OrderBy(x => x.SchoolClass.SortOrder).ThenBy(x => x.NameEn)
        };

        var totalCount = await query.CountAsync(ct);
        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(x => new SectionDto(
                x.Id,
                x.SchoolClassId,
                x.SchoolClass.NameEn,
                x.SchoolClass.NameAr,
                x.NameEn,
                x.NameAr,
                x.DescriptionEn,
                x.DescriptionAr,
                x.IsActive,
                x.Students.Count,
                x.CreatedAt))
            .ToListAsync(ct);

        return new SectionListResult(
            items,
            totalCount,
            page,
            pageSize,
            PaginationHelper.TotalPages(totalCount, pageSize));
    }

    public Task<Section?> GetByIdAsync(Guid tenantId, Guid id, CancellationToken ct) =>
        BaseQuery(tenantId)
            .Include(x => x.Students)
            .FirstOrDefaultAsync(x => x.Id == id, ct);

    public Task<Section?> GetByIdForUpdateAsync(Guid tenantId, Guid id, CancellationToken ct) =>
        db.Sections.FirstOrDefaultAsync(x => x.TenantId == tenantId && x.Id == id, ct);

    public async Task<IReadOnlyList<SchoolClassSectionOptionDto>> GetSectionsBySchoolClassAsync(
        Guid tenantId,
        Guid schoolClassId,
        CancellationToken ct) =>
        await db.Sections.AsNoTracking()
            .Where(x => x.TenantId == tenantId && x.SchoolClassId == schoolClassId && x.IsActive)
            .OrderBy(x => x.NameEn)
            .Select(x => new SchoolClassSectionOptionDto(x.Id, x.NameEn, x.NameAr))
            .ToListAsync(ct);

    public Task<bool> SchoolClassExistsAsync(Guid tenantId, Guid schoolClassId, CancellationToken ct) =>
        db.SchoolClasses.AnyAsync(x => x.TenantId == tenantId && x.Id == schoolClassId, ct);

    public async Task<bool> HasLinkedStudentsTeachersOrTimetableAsync(Guid tenantId, Guid id, CancellationToken ct)
    {
        if (await db.Students.AnyAsync(s => s.TenantId == tenantId && s.SectionId == id, ct))
            return true;

        if (await db.TeacherAssignments.AnyAsync(ta => ta.SectionId == id, ct))
            return true;

        return await db.TimetableEntries.AnyAsync(te => te.TenantId == tenantId && te.SectionId == id, ct);
    }

    public async Task AddAsync(Section entity, CancellationToken ct) =>
        await db.Sections.AddAsync(entity, ct);

    public Task DeleteAsync(Section entity, CancellationToken ct)
    {
        db.Sections.Remove(entity);
        return Task.CompletedTask;
    }

    public Task SaveChangesAsync(CancellationToken ct) => db.SaveChangesAsync(ct);
}
