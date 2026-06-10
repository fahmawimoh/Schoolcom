using FluentValidation;
using MediatR;
using SchoolCom.Application.Common.Interfaces;

namespace SchoolCom.Application.Features.Sections.Commands.DeleteSection;

public record DeleteSectionCommand(Guid Id) : IRequest;

public class DeleteSectionCommandValidator : AbstractValidator<DeleteSectionCommand>
{
    public DeleteSectionCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
    }
}

public class DeleteSectionCommandHandler(
    ISectionRepository repo,
    ICurrentUserService currentUser,
    IAuditService audit) : IRequestHandler<DeleteSectionCommand>
{
    public async Task Handle(DeleteSectionCommand request, CancellationToken cancellationToken)
    {
        var tenantId = currentUser.TenantId
            ?? throw new UnauthorizedAccessException("Tenant not found");

        var entity = await repo.GetByIdForUpdateAsync(tenantId, request.Id, cancellationToken)
            ?? throw new KeyNotFoundException("Section not found");

        if (await repo.HasLinkedClassSectionsAsync(tenantId, request.Id, cancellationToken))
            throw new InvalidOperationException("Cannot delete a section linked to school classes. Remove the links first or deactivate the section.");

        await repo.DeleteAsync(entity, cancellationToken);
        await repo.SaveChangesAsync(cancellationToken);
        await audit.LogAsync("Delete", "Section", entity.Id, ct: cancellationToken);
    }
}
