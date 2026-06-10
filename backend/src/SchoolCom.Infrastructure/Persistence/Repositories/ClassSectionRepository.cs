using Microsoft.EntityFrameworkCore;
using SchoolCom.Application.Common;
using SchoolCom.Application.Common.Exceptions;
using SchoolCom.Application.Features.ClassSections;
using SchoolCom.Application.Features.Sections;
using SchoolCom.Domain.Entities;

namespace SchoolCom.Infrastructure.Persistence.Repositories;

public class ClassSectionRepository(ApplicationDbContext db) : IClassSectionRepository
{
    public Task<ClassSection?> GetByIdAsync(Guid tenantId, Guid id, CancellationToken ct) =>
        db.ClassSections.AsNoTracking()
            .Include(x => x.SchoolClass)
            .Include(x => x.Section)
            .FirstOrDefaultAsync(x => x.TenantId == tenantId && x.Id == id, ct);

    public Task<ClassSection?> GetByClassAndSectionAsync(
        Guid tenantId,
        Guid schoolClassId,
        Guid sectionId,
        CancellationToken ct) =>
        db.ClassSections.AsNoTracking()
            .FirstOrDefaultAsync(x =>
                x.TenantId == tenantId &&
                x.SchoolClassId == schoolClassId &&
                x.SectionId == sectionId, ct);

    public Task<bool> LinkExistsAsync(
        Guid tenantId,
        Guid schoolClassId,
        Guid sectionId,
        CancellationToken ct) =>
        db.ClassSections.AnyAsync(x =>
            x.TenantId == tenantId &&
            x.SchoolClassId == schoolClassId &&
            x.SectionId == sectionId, ct);

    public async Task<IReadOnlyList<ClassSectionOptionDto>> GetBySchoolClassAsync(
        Guid tenantId,
        Guid schoolClassId,
        CancellationToken ct) =>
        await db.ClassSections.AsNoTracking()
            .Where(x => x.TenantId == tenantId && x.SchoolClassId == schoolClassId && x.IsActive)
            .OrderBy(x => x.Section.NameEn)
            .Select(x => new ClassSectionOptionDto(
                x.Id,
                x.SectionId,
                x.Section.NameEn,
                x.Section.NameAr))
            .ToListAsync(ct);

    public async Task<IReadOnlyList<Guid>> GetLinkedSectionIdsAsync(
        Guid tenantId,
        Guid schoolClassId,
        CancellationToken ct) =>
        await db.ClassSections.AsNoTracking()
            .Where(x => x.TenantId == tenantId && x.SchoolClassId == schoolClassId)
            .Select(x => x.SectionId)
            .ToListAsync(ct);

    public async Task SyncLinksAsync(
        Guid tenantId,
        Guid schoolClassId,
        IReadOnlyList<Guid> sectionIds,
        CancellationToken ct)
    {
        var existing = await db.ClassSections
            .Where(x => x.TenantId == tenantId && x.SchoolClassId == schoolClassId)
            .ToListAsync(ct);

        var desired = sectionIds.Distinct().ToHashSet();
        var existingIds = existing.Select(x => x.SectionId).ToHashSet();

        foreach (var link in existing.Where(x => !desired.Contains(x.SectionId)))
            db.ClassSections.Remove(link);

        foreach (var sectionId in desired.Where(id => !existingIds.Contains(id)))
        {
            if (await LinkExistsAsync(tenantId, schoolClassId, sectionId, ct))
            {
                throw new ConflictException(
                    AcademicErrorCodes.ClassSectionDuplicate,
                    AcademicErrorMessages.ClassSectionDuplicateEn,
                    AcademicErrorMessages.ClassSectionDuplicateAr,
                    AcademicTranslationKeys.ClassSectionDuplicate);
            }

            await db.ClassSections.AddAsync(new ClassSection
            {
                TenantId = tenantId,
                SchoolClassId = schoolClassId,
                SectionId = sectionId,
                IsActive = true
            }, ct);
        }
    }

    public async Task<bool> AllSectionsExistAsync(
        Guid tenantId,
        IReadOnlyList<Guid> sectionIds,
        CancellationToken ct)
    {
        if (sectionIds.Count == 0) return false;
        var count = await db.Sections.CountAsync(
            x => x.TenantId == tenantId && sectionIds.Contains(x.Id), ct);
        return count == sectionIds.Distinct().Count();
    }

    public Task<int> CountBySchoolClassAsync(Guid tenantId, Guid schoolClassId, CancellationToken ct) =>
        db.ClassSections.CountAsync(
            x => x.TenantId == tenantId && x.SchoolClassId == schoolClassId, ct);

    public async Task RemoveAllBySchoolClassAsync(
        Guid tenantId,
        Guid schoolClassId,
        CancellationToken ct)
    {
        var links = await db.ClassSections
            .Where(x => x.TenantId == tenantId && x.SchoolClassId == schoolClassId)
            .ToListAsync(ct);

        db.ClassSections.RemoveRange(links);
    }
}
