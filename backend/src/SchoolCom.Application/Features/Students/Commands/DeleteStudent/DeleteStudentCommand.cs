using FluentValidation;
using MediatR;
using SchoolCom.Application.Common.Interfaces;

namespace SchoolCom.Application.Features.Students.Commands.DeleteStudent;

public record DeleteStudentCommand(Guid Id) : IRequest<Unit>;

public class DeleteStudentCommandValidator : AbstractValidator<DeleteStudentCommand>
{
    public DeleteStudentCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
    }
}

public class DeleteStudentCommandHandler(
    IStudentRepository students,
    ICurrentUserService currentUser,
    IAuditService audit) : IRequestHandler<DeleteStudentCommand, Unit>
{
    public async Task<Unit> Handle(DeleteStudentCommand request, CancellationToken cancellationToken)
    {
        var tenantId = currentUser.TenantId
            ?? throw new UnauthorizedAccessException("Tenant not found");

        var student = await students.GetByIdForUpdateAsync(tenantId, request.Id, cancellationToken)
            ?? throw new KeyNotFoundException($"Student {request.Id} not found");

        await students.DeleteAsync(student, cancellationToken);
        await students.SaveChangesAsync(cancellationToken);
        await audit.LogAsync("Delete", "Student", request.Id, ct: cancellationToken);

        return Unit.Value;
    }
}
