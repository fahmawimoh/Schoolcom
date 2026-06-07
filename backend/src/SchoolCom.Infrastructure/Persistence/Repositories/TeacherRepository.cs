using Microsoft.EntityFrameworkCore;
using SchoolCom.Application.Common;
using SchoolCom.Application.Features.Teachers;
using SchoolCom.Domain.Entities;

namespace SchoolCom.Infrastructure.Persistence.Repositories;

public class TeacherRepository(ApplicationDbContext db) : ITeacherRepository
{
    private IQueryable<Teacher> BaseQuery(Guid tenantId) =>
        db.Teachers.AsNoTracking().Where(x => x.TenantId == tenantId);

    public async Task<TeacherListResult> GetPagedAsync(
        Guid tenantId,
        int page,
        int pageSize,
        string? search,
        bool? isActive,
        string? subject,
        string? sortBy,
        bool sortDesc,
        CancellationToken ct)
    {
        var query = BaseQuery(tenantId);

        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim().ToLower();
            query = query.Where(x =>
                x.FirstNameEn.ToLower().Contains(term) ||
                x.FirstNameAr.Contains(term) ||
                x.LastNameEn.ToLower().Contains(term) ||
                x.LastNameAr.Contains(term) ||
                x.Email.ToLower().Contains(term) ||
                x.EmployeeNumber.ToLower().Contains(term));
        }

        if (isActive.HasValue)
            query = query.Where(x => x.IsActive == isActive.Value);

        if (!string.IsNullOrWhiteSpace(subject))
        {
            var subjectTerm = subject.Trim().ToLower();
            query = query.Where(x =>
                x.SubjectEn.ToLower().Contains(subjectTerm) ||
                x.SubjectAr.Contains(subjectTerm));
        }

        query = (sortBy?.ToLower()) switch
        {
            "firstnameen" => sortDesc ? query.OrderByDescending(x => x.FirstNameEn) : query.OrderBy(x => x.FirstNameEn),
            "lastnameen" => sortDesc ? query.OrderByDescending(x => x.LastNameEn) : query.OrderBy(x => x.LastNameEn),
            "email" => sortDesc ? query.OrderByDescending(x => x.Email) : query.OrderBy(x => x.Email),
            "employeenumber" => sortDesc ? query.OrderByDescending(x => x.EmployeeNumber) : query.OrderBy(x => x.EmployeeNumber),
            "subjecten" => sortDesc ? query.OrderByDescending(x => x.SubjectEn) : query.OrderBy(x => x.SubjectEn),
            "createdat" => sortDesc ? query.OrderByDescending(x => x.CreatedAt) : query.OrderBy(x => x.CreatedAt),
            _ => sortDesc ? query.OrderByDescending(x => x.LastNameEn) : query.OrderBy(x => x.LastNameEn)
        };

        var totalCount = await query.CountAsync(ct);
        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(x => new TeacherDto(
                x.Id,
                x.FirstNameEn,
                x.FirstNameAr,
                x.LastNameEn,
                x.LastNameAr,
                x.Email,
                x.Phone,
                x.EmployeeNumber,
                x.SubjectEn,
                x.SubjectAr,
                x.IsActive,
                x.UserId,
                x.Assignments.Select(a => new TeacherAssignmentDto(
                    a.Id,
                    a.SchoolClassId,
                    a.SectionId,
                    a.SubjectId,
                    a.SchoolClass.NameEn,
                    a.SchoolClass.NameAr,
                    a.Section != null ? a.Section.NameEn : null,
                    a.Section != null ? a.Section.NameAr : null,
                    a.Subject.NameEn,
                    a.Subject.NameAr)).ToList(),
                x.CreatedAt))
            .ToListAsync(ct);

        return new TeacherListResult(
            items,
            totalCount,
            page,
            pageSize,
            PaginationHelper.TotalPages(totalCount, pageSize));
    }

    public Task<Teacher?> GetByIdAsync(Guid tenantId, Guid id, CancellationToken ct) =>
        db.Teachers.AsNoTracking()
            .Include(x => x.Assignments)
            .ThenInclude(a => a.SchoolClass)
            .Include(x => x.Assignments)
            .ThenInclude(a => a.Section)
            .Include(x => x.Assignments)
            .ThenInclude(a => a.Subject)
            .FirstOrDefaultAsync(x => x.TenantId == tenantId && x.Id == id, ct);

    public Task<Teacher?> GetByIdForUpdateAsync(Guid tenantId, Guid id, CancellationToken ct) =>
        db.Teachers.FirstOrDefaultAsync(x => x.TenantId == tenantId && x.Id == id, ct);

    public Task<bool> EmployeeNumberExistsAsync(
        Guid tenantId,
        string employeeNumber,
        Guid? excludeId,
        CancellationToken ct)
    {
        var normalized = employeeNumber.Trim();
        return db.Teachers.AnyAsync(
            x => x.TenantId == tenantId &&
                 x.EmployeeNumber == normalized &&
                 (!excludeId.HasValue || x.Id != excludeId.Value),
            ct);
    }

    public Task<bool> UserBelongsToTenantAsync(Guid tenantId, Guid userId, CancellationToken ct) =>
        db.Users.AnyAsync(u => u.Id == userId && u.TenantId == tenantId, ct);

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

    public Task<bool> SubjectExistsAsync(Guid tenantId, Guid subjectId, CancellationToken ct) =>
        db.Subjects.AnyAsync(x => x.TenantId == tenantId && x.Id == subjectId, ct);

    public async Task ReplaceAssignmentsAsync(
        Guid tenantId,
        Guid teacherId,
        IReadOnlyList<TeacherAssignmentInput> assignments,
        CancellationToken ct)
    {
        var teacherExists = await db.Teachers.AnyAsync(
            t => t.TenantId == tenantId && t.Id == teacherId, ct);
        if (!teacherExists)
            throw new KeyNotFoundException("Teacher not found");

        await using var transaction = await db.Database.BeginTransactionAsync(ct);

        var existing = await db.TeacherAssignments
            .Where(a => a.TeacherId == teacherId)
            .ToListAsync(ct);
        db.TeacherAssignments.RemoveRange(existing);

        var distinctAssignments = assignments
            .GroupBy(a => (a.SchoolClassId, a.SectionId, a.SubjectId))
            .Select(g => g.First())
            .ToList();

        foreach (var assignment in distinctAssignments)
        {
            await db.TeacherAssignments.AddAsync(new TeacherAssignment
            {
                TeacherId = teacherId,
                SchoolClassId = assignment.SchoolClassId,
                SectionId = assignment.SectionId,
                SubjectId = assignment.SubjectId
            }, ct);
        }

        await db.SaveChangesAsync(ct);
        await transaction.CommitAsync(ct);
    }

    public async Task AddAsync(Teacher entity, CancellationToken ct) =>
        await db.Teachers.AddAsync(entity, ct);

    public Task DeleteAsync(Teacher entity, CancellationToken ct)
    {
        db.Teachers.Remove(entity);
        return Task.CompletedTask;
    }

    public Task SaveChangesAsync(CancellationToken ct) => db.SaveChangesAsync(ct);
}
