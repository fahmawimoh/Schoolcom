using Microsoft.EntityFrameworkCore;
using SchoolCom.Application.Common;
using SchoolCom.Application.Features.RoomAssignments;
using SchoolCom.Domain.Entities;

namespace SchoolCom.Infrastructure.Persistence.Repositories;

public class RoomAssignmentRepository(ApplicationDbContext db) : IRoomAssignmentRepository
{
    private IQueryable<RoomAssignment> BaseQuery(Guid tenantId) =>
        db.RoomAssignments.AsNoTracking()
            .Include(x => x.ClassSection).ThenInclude(cs => cs.SchoolClass)
            .Include(x => x.ClassSection).ThenInclude(cs => cs.Section)
            .Include(x => x.Room)
            .Where(x => x.TenantId == tenantId);

    public async Task<RoomAssignmentListResult> GetPagedAsync(
        Guid tenantId,
        int page,
        int pageSize,
        Guid? schoolClassId,
        Guid? sectionId,
        Guid? roomId,
        CancellationToken ct)
    {
        var query = BaseQuery(tenantId);

        if (schoolClassId.HasValue)
            query = query.Where(x => x.ClassSection.SchoolClassId == schoolClassId.Value);

        if (sectionId.HasValue)
            query = query.Where(x => x.ClassSection.SectionId == sectionId.Value);

        if (roomId.HasValue)
            query = query.Where(x => x.RoomId == roomId.Value);

        query = query.OrderBy(x => x.ClassSection.SchoolClass.SortOrder)
            .ThenBy(x => x.ClassSection.Section.NameEn);

        var totalCount = await query.CountAsync(ct);
        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(x => new RoomAssignmentDto(
                x.Id,
                x.ClassSectionId,
                x.ClassSection.SchoolClassId,
                x.ClassSection.SchoolClass.NameEn,
                x.ClassSection.SchoolClass.NameAr,
                x.ClassSection.SectionId,
                x.ClassSection.Section.NameEn,
                x.ClassSection.Section.NameAr,
                x.RoomId,
                x.Room.NameEn,
                x.Room.NameAr,
                x.Room.Code,
                x.CreatedAt))
            .ToListAsync(ct);

        return new RoomAssignmentListResult(
            items,
            totalCount,
            page,
            pageSize,
            PaginationHelper.TotalPages(totalCount, pageSize));
    }

    public Task<RoomAssignment?> GetByIdAsync(Guid tenantId, Guid id, CancellationToken ct) =>
        BaseQuery(tenantId).FirstOrDefaultAsync(x => x.Id == id, ct);

    public Task<RoomAssignment?> GetByIdForUpdateAsync(Guid tenantId, Guid id, CancellationToken ct) =>
        db.RoomAssignments.FirstOrDefaultAsync(x => x.TenantId == tenantId && x.Id == id, ct);

    public Task<bool> ClassSectionExistsAsync(Guid tenantId, Guid classSectionId, CancellationToken ct) =>
        db.ClassSections.AnyAsync(x => x.TenantId == tenantId && x.Id == classSectionId, ct);

    public Task<bool> RoomExistsAsync(Guid tenantId, Guid roomId, CancellationToken ct) =>
        db.Rooms.AnyAsync(x => x.TenantId == tenantId && x.Id == roomId, ct);

    public Task<bool> ClassSectionAlreadyAssignedAsync(
        Guid tenantId,
        Guid classSectionId,
        Guid? excludeId,
        CancellationToken ct)
    {
        var query = db.RoomAssignments.Where(x =>
            x.TenantId == tenantId && x.ClassSectionId == classSectionId);

        if (excludeId.HasValue)
            query = query.Where(x => x.Id != excludeId.Value);

        return query.AnyAsync(ct);
    }

    public Task<bool> RoomAlreadyAssignedAsync(
        Guid tenantId,
        Guid roomId,
        Guid? excludeId,
        CancellationToken ct)
    {
        var query = db.RoomAssignments.Where(x =>
            x.TenantId == tenantId && x.RoomId == roomId);

        if (excludeId.HasValue)
            query = query.Where(x => x.Id != excludeId.Value);

        return query.AnyAsync(ct);
    }

    public Task<bool> ExistsForClassSectionAndRoomNameAsync(
        Guid tenantId,
        Guid classSectionId,
        string nameEn,
        Guid? excludeRoomId,
        CancellationToken ct)
    {
        var normalizedName = nameEn.Trim().ToLower();

        var query = db.RoomAssignments.Where(ra =>
            ra.TenantId == tenantId &&
            ra.ClassSectionId == classSectionId &&
            ra.Room.NameEn.ToLower() == normalizedName);

        if (excludeRoomId.HasValue)
            query = query.Where(ra => ra.RoomId != excludeRoomId.Value);

        return query.AnyAsync(ct);
    }

    public Task<RoomAssignment?> GetByRoomIdForUpdateAsync(
        Guid tenantId,
        Guid roomId,
        CancellationToken ct) =>
        db.RoomAssignments.FirstOrDefaultAsync(x => x.TenantId == tenantId && x.RoomId == roomId, ct);

    public async Task<IReadOnlyList<ClassSectionLookupDto>> GetClassSectionOptionsAsync(
        Guid tenantId,
        Guid? schoolClassId,
        CancellationToken ct)
    {
        var query = db.ClassSections.AsNoTracking()
            .Where(x => x.TenantId == tenantId && x.IsActive);

        if (schoolClassId.HasValue)
            query = query.Where(x => x.SchoolClassId == schoolClassId.Value);

        return await query
            .OrderBy(x => x.SchoolClass.SortOrder)
            .ThenBy(x => x.Section.NameEn)
            .Select(x => new ClassSectionLookupDto(
                x.Id,
                x.SchoolClassId,
                x.SchoolClass.NameEn,
                x.SchoolClass.NameAr,
                x.SectionId,
                x.Section.NameEn,
                x.Section.NameAr))
            .ToListAsync(ct);
    }

    public async Task AddAsync(RoomAssignment entity, CancellationToken ct) =>
        await db.RoomAssignments.AddAsync(entity, ct);

    public Task DeleteAsync(RoomAssignment entity, CancellationToken ct)
    {
        db.RoomAssignments.Remove(entity);
        return Task.CompletedTask;
    }

    public Task SaveChangesAsync(CancellationToken ct) => db.SaveChangesAsync(ct);
}
