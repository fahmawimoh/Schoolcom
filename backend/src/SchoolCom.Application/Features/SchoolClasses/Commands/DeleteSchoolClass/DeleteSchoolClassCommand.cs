using FluentValidation;
using MediatR;
using SchoolCom.Application.Common.Interfaces;
using SchoolCom.Application.Features.ClassSections;

namespace SchoolCom.Application.Features.SchoolClasses.Commands.DeleteSchoolClass;

public record DeleteSchoolClassCommand(Guid Id) : IRequest;

public class DeleteSchoolClassCommandValidator : AbstractValidator<DeleteSchoolClassCommand>
{
    public DeleteSchoolClassCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
    }
}

public class DeleteSchoolClassCommandHandler(
    ISchoolClassRepository repo,
    IClassSectionRepository classSectionRepo,
    ICurrentUserService currentUser,
    IAuditService audit) : IRequestHandler<DeleteSchoolClassCommand>
{
    public async Task Handle(DeleteSchoolClassCommand request, CancellationToken cancellationToken)
    {
        var tenantId = currentUser.TenantId
            ?? throw new UnauthorizedAccessException("Tenant not found");

        var entity = await repo.GetByIdForUpdateAsync(tenantId, request.Id, cancellationToken)
            ?? throw new KeyNotFoundException("School class not found");

        if (await repo.HasLinkedStudentsOrAssignmentsAsync(tenantId, request.Id, cancellationToken))
            throw new InvalidOperationException(
                "Cannot delete a school class that has linked students, teachers, timetables, or room assignments. Deactivate it instead.");

        await classSectionRepo.RemoveAllBySchoolClassAsync(tenantId, request.Id, cancellationToken);
        await repo.DeleteAsync(entity, cancellationToken);
        await repo.SaveChangesAsync(cancellationToken);
        await audit.LogAsync("Delete", "SchoolClass", entity.Id, ct: cancellationToken);
    }
}
