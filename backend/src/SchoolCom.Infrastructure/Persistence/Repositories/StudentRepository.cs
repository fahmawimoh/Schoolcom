using Microsoft.EntityFrameworkCore;
using SchoolCom.Application.Common;
using SchoolCom.Application.Features.Students;
using SchoolCom.Domain.Entities;
using SchoolCom.Domain.Enums;

namespace SchoolCom.Infrastructure.Persistence.Repositories;

public class StudentRepository(ApplicationDbContext db) : IStudentRepository
{
    private IQueryable<Student> BaseQuery(Guid tenantId) =>
        db.Students
            .AsNoTracking()
            .Include(s => s.SchoolClass)
            .Include(s => s.Section)
            .Where(s => s.TenantId == tenantId);

    public async Task<StudentListResult> GetPagedAsync(
        Guid tenantId,
        int page,
        int pageSize,
        Guid? schoolClassId,
        Guid? sectionId,
        string? search,
        string? locale,
        CancellationToken ct)
    {
        var query = BaseQuery(tenantId);

        if (schoolClassId.HasValue)
            query = query.Where(s => s.SchoolClassId == schoolClassId.Value);

        if (sectionId.HasValue)
            query = query.Where(s => s.SectionId == sectionId.Value);

        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim().ToLower();
            query = query.Where(s =>
                s.StudentNumber.ToLower().Contains(term) ||
                (s.FirstNameEn + " " + s.LastNameEn).ToLower().Contains(term) ||
                (s.FirstNameAr + " " + s.LastNameAr).Contains(term));
        }

        var totalCount = await query.CountAsync(ct);
        var totalPages = PaginationHelper.TotalPages(totalCount, pageSize);

        var items = await query
            .OrderBy(s => s.StudentNumber)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);

        var dtos = items.Select(s => StudentMapper.ToDto(s, locale)).ToList();
        return new StudentListResult(dtos, totalCount, page, pageSize, totalPages);
    }

    public async Task<StudentFiltersDto> GetFiltersAsync(Guid tenantId, CancellationToken ct)
    {
        var schoolClasses = await db.SchoolClasses
            .AsNoTracking()
            .Where(g => g.TenantId == tenantId && g.IsActive)
            .OrderBy(g => g.SortOrder)
            .Select(g => new SchoolClassFilterOptionDto(g.Id, g.NameEn, g.NameAr))
            .ToListAsync(ct);

        var sections = await db.Sections
            .AsNoTracking()
            .Where(s => s.TenantId == tenantId && s.IsActive)
            .OrderBy(s => s.SchoolClass.SortOrder)
            .ThenBy(s => s.NameEn)
            .Select(s => new SectionFilterOptionDto(s.Id, s.SchoolClassId, s.NameEn, s.NameAr))
            .ToListAsync(ct);

        var parents = await db.Users
            .AsNoTracking()
            .Where(u => u.TenantId == tenantId && u.IsActive &&
                        u.RoleAssignments.Any(r => r.Role.Name == UserRole.Parent))
            .OrderBy(u => u.FirstName)
            .ThenBy(u => u.LastName)
            .Select(u => new ParentFilterOptionDto(u.Id, u.FirstName + " " + u.LastName))
            .ToListAsync(ct);

        return new StudentFiltersDto(schoolClasses, sections, parents);
    }

    public Task<Student?> GetByIdAsync(Guid tenantId, Guid id, CancellationToken ct) =>
        BaseQuery(tenantId).FirstOrDefaultAsync(s => s.Id == id, ct);

    public Task<Student?> GetByIdForUpdateAsync(Guid tenantId, Guid id, CancellationToken ct) =>
        db.Students
            .Include(s => s.SchoolClass)
            .Include(s => s.Section)
            .FirstOrDefaultAsync(s => s.TenantId == tenantId && s.Id == id, ct);

    public Task<bool> StudentNumberExistsAsync(Guid tenantId, string studentNumber, Guid? excludeId, CancellationToken ct) =>
        db.Students.AnyAsync(
            s => s.TenantId == tenantId &&
                 s.StudentNumber == studentNumber &&
                 (!excludeId.HasValue || s.Id != excludeId.Value),
            ct);

    public async Task<string> GenerateNextStudentNumberAsync(Guid tenantId, CancellationToken ct)
    {
        var year = DateTime.UtcNow.Year;
        var prefix = $"STU-{year}-";

        var numbers = await db.Students
            .AsNoTracking()
            .Where(s => s.TenantId == tenantId && s.StudentNumber.StartsWith(prefix))
            .Select(s => s.StudentNumber)
            .ToListAsync(ct);

        var maxSequence = 0;
        foreach (var number in numbers)
        {
            var suffix = number[prefix.Length..];
            if (int.TryParse(suffix, out var sequence))
                maxSequence = Math.Max(maxSequence, sequence);
        }

        return $"{prefix}{(maxSequence + 1):D3}";
    }

    public Task<bool> SectionBelongsToSchoolClassAsync(Guid tenantId, Guid schoolClassId, Guid sectionId, CancellationToken ct) =>
        db.Sections.AnyAsync(
            s => s.TenantId == tenantId && s.SchoolClassId == schoolClassId && s.Id == sectionId,
            ct);

    public async Task AddAsync(Student student, CancellationToken ct) =>
        await db.Students.AddAsync(student, ct);

    public Task DeleteAsync(Student student, CancellationToken ct)
    {
        db.Students.Remove(student);
        return Task.CompletedTask;
    }

    public Task SaveChangesAsync(CancellationToken ct) => db.SaveChangesAsync(ct);
}
