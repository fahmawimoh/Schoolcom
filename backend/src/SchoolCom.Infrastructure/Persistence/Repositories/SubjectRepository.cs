using Microsoft.EntityFrameworkCore;
using SchoolCom.Application.Common;
using SchoolCom.Application.Features.Subjects;
using SchoolCom.Domain.Entities;

namespace SchoolCom.Infrastructure.Persistence.Repositories;

public class SubjectRepository(ApplicationDbContext db) : ISubjectRepository
{
    private IQueryable<Subject> BaseQuery(Guid tenantId) =>
        db.Subjects.AsNoTracking().Where(x => x.TenantId == tenantId);

    public async Task<SubjectListResult> GetPagedAsync(
        Guid tenantId,
        int page,
        int pageSize,
        string? search,
        bool? isActive,
        string? sortBy,
        bool sortDesc,
        CancellationToken ct)
    {
        var query = BaseQuery(tenantId);

        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim().ToLower();
            query = query.Where(x =>
                x.NameEn.ToLower().Contains(term) ||
                x.NameAr.Contains(term) ||
                (x.Code != null && x.Code.ToLower().Contains(term)));
        }

        if (isActive.HasValue)
            query = query.Where(x => x.IsActive == isActive.Value);

        query = (sortBy?.ToLower()) switch
        {
            "nameen" => sortDesc ? query.OrderByDescending(x => x.NameEn) : query.OrderBy(x => x.NameEn),
            "namear" => sortDesc ? query.OrderByDescending(x => x.NameAr) : query.OrderBy(x => x.NameAr),
            "code" => sortDesc ? query.OrderByDescending(x => x.Code) : query.OrderBy(x => x.Code),
            "createdat" => sortDesc ? query.OrderByDescending(x => x.CreatedAt) : query.OrderBy(x => x.CreatedAt),
            _ => sortDesc ? query.OrderByDescending(x => x.NameEn) : query.OrderBy(x => x.NameEn)
        };

        var totalCount = await query.CountAsync(ct);
        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(x => new SubjectDto(x.Id, x.NameEn, x.NameAr, x.Code, x.IsActive, x.CreatedAt))
            .ToListAsync(ct);

        return new SubjectListResult(
            items,
            totalCount,
            page,
            pageSize,
            PaginationHelper.TotalPages(totalCount, pageSize));
    }

    public Task<Subject?> GetByIdAsync(Guid tenantId, Guid id, CancellationToken ct) =>
        BaseQuery(tenantId).FirstOrDefaultAsync(x => x.Id == id, ct);

    public Task<Subject?> GetByIdForUpdateAsync(Guid tenantId, Guid id, CancellationToken ct) =>
        db.Subjects.FirstOrDefaultAsync(x => x.TenantId == tenantId && x.Id == id, ct);

    public async Task<bool> HasLinkedAssignmentsOrTimetableAsync(Guid tenantId, Guid id, CancellationToken ct)
    {
        if (await db.TeacherAssignments.AnyAsync(ta => ta.SubjectId == id, ct))
            return true;

        return await db.TimetableEntries.AnyAsync(te => te.TenantId == tenantId && te.SubjectId == id, ct);
    }

    public async Task AddAsync(Subject entity, CancellationToken ct) =>
        await db.Subjects.AddAsync(entity, ct);

    public Task DeleteAsync(Subject entity, CancellationToken ct)
    {
        db.Subjects.Remove(entity);
        return Task.CompletedTask;
    }

    public Task SaveChangesAsync(CancellationToken ct) => db.SaveChangesAsync(ct);
}
