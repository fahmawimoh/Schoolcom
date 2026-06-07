using SchoolCom.Domain.Entities;

namespace SchoolCom.Application.Features.Subjects;

public interface ISubjectRepository
{
    Task<SubjectListResult> GetPagedAsync(
        Guid tenantId,
        int page,
        int pageSize,
        string? search,
        bool? isActive,
        string? sortBy,
        bool sortDesc,
        CancellationToken ct);

    Task<Subject?> GetByIdAsync(Guid tenantId, Guid id, CancellationToken ct);

    Task<Subject?> GetByIdForUpdateAsync(Guid tenantId, Guid id, CancellationToken ct);

    Task<bool> HasLinkedAssignmentsOrTimetableAsync(Guid tenantId, Guid id, CancellationToken ct);

    Task AddAsync(Subject entity, CancellationToken ct);

    Task DeleteAsync(Subject entity, CancellationToken ct);

    Task SaveChangesAsync(CancellationToken ct);
}
