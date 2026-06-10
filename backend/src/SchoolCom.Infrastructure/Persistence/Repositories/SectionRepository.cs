using Microsoft.EntityFrameworkCore;
using SchoolCom.Application.Common;
using SchoolCom.Application.Features.Sections;
using SchoolCom.Domain.Entities;

namespace SchoolCom.Infrastructure.Persistence.Repositories;

public class SectionRepository(ApplicationDbContext db) : ISectionRepository
{
    private IQueryable<Section> BaseQuery(Guid tenantId) =>
        db.Sections.AsNoTracking().Where(x => x.TenantId == tenantId);

    public async Task<SectionListResult> GetPagedAsync(
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
            _ => sortDesc ? query.OrderByDescending(x => x.NameEn) : query.OrderBy(x => x.NameEn)
        };

        var totalCount = await query.CountAsync(ct);
        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(x => new SectionDto(
                x.Id,
                x.NameEn,
                x.NameAr,
                x.DescriptionEn,
                x.DescriptionAr,
                x.IsActive,
                x.ClassSections.Count,
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
            .Include(x => x.ClassSections)
            .FirstOrDefaultAsync(x => x.Id == id, ct);

    public Task<Section?> GetByIdForUpdateAsync(Guid tenantId, Guid id, CancellationToken ct) =>
        db.Sections.FirstOrDefaultAsync(x => x.TenantId == tenantId && x.Id == id, ct);

    public Task<bool> NameExistsAsync(
        Guid tenantId,
        string nameEn,
        Guid? excludeSectionId,
        CancellationToken ct)
    {
        var normalized = nameEn.Trim().ToLower();
        var query = db.Sections.Where(x =>
            x.TenantId == tenantId &&
            x.NameEn.ToLower() == normalized);

        if (excludeSectionId.HasValue)
            query = query.Where(x => x.Id != excludeSectionId.Value);

        return query.AnyAsync(ct);
    }

    public Task<bool> HasLinkedClassSectionsAsync(Guid tenantId, Guid id, CancellationToken ct) =>
        db.ClassSections.AnyAsync(x => x.TenantId == tenantId && x.SectionId == id, ct);

    public async Task AddAsync(Section entity, CancellationToken ct) =>
        await db.Sections.AddAsync(entity, ct);

    public Task DeleteAsync(Section entity, CancellationToken ct)
    {
        db.Sections.Remove(entity);
        return Task.CompletedTask;
    }

    public Task SaveChangesAsync(CancellationToken ct) => db.SaveChangesAsync(ct);
}
