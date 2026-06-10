using Microsoft.EntityFrameworkCore;
using SchoolCom.Application.Common;
using SchoolCom.Application.Features.Rooms;
using SchoolCom.Domain.Entities;

namespace SchoolCom.Infrastructure.Persistence.Repositories;

public class RoomRepository(ApplicationDbContext db) : IRoomRepository
{
    private IQueryable<Room> BaseQuery(Guid tenantId) =>
        db.Rooms.AsNoTracking().Where(x => x.TenantId == tenantId);

    public async Task<RoomListResult> GetPagedAsync(
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
            "capacity" => sortDesc ? query.OrderByDescending(x => x.Capacity) : query.OrderBy(x => x.Capacity),
            "createdat" => sortDesc ? query.OrderByDescending(x => x.CreatedAt) : query.OrderBy(x => x.CreatedAt),
            _ => sortDesc ? query.OrderByDescending(x => x.NameEn) : query.OrderBy(x => x.NameEn)
        };

        var totalCount = await query.CountAsync(ct);
        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(x => new RoomDto(
                x.Id,
                x.NameEn,
                x.NameAr,
                x.Code,
                x.Capacity,
                x.IsActive,
                x.RoomAssignments.Select(ra => (Guid?)ra.ClassSection.SchoolClassId).FirstOrDefault(),
                x.RoomAssignments.Select(ra => (Guid?)ra.ClassSection.SectionId).FirstOrDefault(),
                x.RoomAssignments.Select(ra => (Guid?)ra.ClassSectionId).FirstOrDefault(),
                x.CreatedAt))
            .ToListAsync(ct);

        return new RoomListResult(
            items,
            totalCount,
            page,
            pageSize,
            PaginationHelper.TotalPages(totalCount, pageSize));
    }

    public async Task<Room?> GetByIdAsync(Guid tenantId, Guid id, CancellationToken ct) =>
        await BaseQuery(tenantId)
            .Include(x => x.RoomAssignments)
            .ThenInclude(ra => ra.ClassSection)
            .FirstOrDefaultAsync(x => x.Id == id, ct);

    public Task<Room?> GetByIdForUpdateAsync(Guid tenantId, Guid id, CancellationToken ct) =>
        db.Rooms.FirstOrDefaultAsync(x => x.TenantId == tenantId && x.Id == id, ct);

    public Task<bool> HasLinkedTimetableEntriesAsync(Guid tenantId, Guid id, CancellationToken ct) =>
        db.TimetableEntries.AnyAsync(te => te.TenantId == tenantId && te.RoomId == id, ct);

    public Task<bool> CodeExistsAsync(
        Guid tenantId,
        string? code,
        Guid? excludeRoomId,
        CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(code)) return Task.FromResult(false);

        var normalized = code.Trim().ToLower();
        var query = db.Rooms.Where(x =>
            x.TenantId == tenantId &&
            x.Code != null &&
            x.Code.ToLower() == normalized);

        if (excludeRoomId.HasValue)
            query = query.Where(x => x.Id != excludeRoomId.Value);

        return query.AnyAsync(ct);
    }

    public async Task<string> GenerateUniqueRoomCodeAsync(Guid tenantId, CancellationToken ct)
    {
        var existingCodes = await db.Rooms
            .Where(x => x.TenantId == tenantId && x.Code != null && x.Code.StartsWith("RM-"))
            .Select(x => x.Code!)
            .ToListAsync(ct);

        var maxSequence = existingCodes
            .Select(code =>
            {
                var suffix = code.Length > 3 ? code[3..] : "";
                return int.TryParse(suffix, out var n) ? n : 0;
            })
            .DefaultIfEmpty(0)
            .Max();

        for (var offset = 1; offset <= 1000; offset++)
        {
            var candidate = $"RM-{(maxSequence + offset):D6}";
            if (!await CodeExistsAsync(tenantId, candidate, excludeRoomId: null, ct))
                return candidate;
        }

        throw new InvalidOperationException("Unable to generate a unique room code.");
    }

    public Task<RoomAssignment?> GetAssignmentByRoomIdAsync(Guid tenantId, Guid roomId, CancellationToken ct) =>
        db.RoomAssignments
            .Include(x => x.ClassSection)
            .FirstOrDefaultAsync(x => x.TenantId == tenantId && x.RoomId == roomId, ct);

    public async Task AddAsync(Room entity, CancellationToken ct) =>
        await db.Rooms.AddAsync(entity, ct);

    public Task DeleteAsync(Room entity, CancellationToken ct)
    {
        db.Rooms.Remove(entity);
        return Task.CompletedTask;
    }

    public Task SaveChangesAsync(CancellationToken ct) => db.SaveChangesAsync(ct);
}
