using FluentValidation;
using MediatR;
using SchoolCom.Application.Common.Interfaces;

namespace SchoolCom.Application.Features.Sections.Queries.GetSections;

public record GetSectionsQuery(
    int Page = 1,
    int PageSize = 10,
    Guid? SchoolClassId = null,
    string? Search = null,
    bool? IsActive = null,
    string? SortBy = null,
    bool SortDesc = false) : IRequest<SectionListResult>;

public class GetSectionsQueryValidator : AbstractValidator<GetSectionsQuery>
{
    public GetSectionsQueryValidator()
    {
        RuleFor(x => x.Page).GreaterThanOrEqualTo(1);
        RuleFor(x => x.PageSize).InclusiveBetween(1, 100);
    }
}

public class GetSectionsQueryHandler(
    ISectionRepository repo,
    ICurrentUserService currentUser) : IRequestHandler<GetSectionsQuery, SectionListResult>
{
    public async Task<SectionListResult> Handle(GetSectionsQuery request, CancellationToken ct)
    {
        var tenantId = currentUser.TenantId
            ?? throw new UnauthorizedAccessException("Tenant not found");

        return await repo.GetPagedAsync(
            tenantId,
            request.Page,
            request.PageSize,
            request.SchoolClassId,
            request.Search,
            request.IsActive,
            request.SortBy,
            request.SortDesc,
            ct);
    }
}
