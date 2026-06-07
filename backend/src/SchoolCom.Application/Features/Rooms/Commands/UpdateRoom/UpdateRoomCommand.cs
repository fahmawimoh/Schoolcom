using FluentValidation;
using MediatR;
using SchoolCom.Application.Common.Interfaces;

namespace SchoolCom.Application.Features.Rooms.Commands.UpdateRoom;

public record UpdateRoomCommand(
    Guid Id,
    string NameEn,
    string NameAr,
    string? Code,
    int? Capacity,
    bool IsActive) : IRequest<RoomDto>;

public class UpdateRoomCommandValidator : AbstractValidator<UpdateRoomCommand>
{
    public UpdateRoomCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
        RuleFor(x => x.NameEn).NotEmpty().MaximumLength(100);
        RuleFor(x => x.NameAr).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Code).MaximumLength(32);
        RuleFor(x => x.Capacity).GreaterThan(0).When(x => x.Capacity.HasValue);
    }
}

public class UpdateRoomCommandHandler(
    IRoomRepository repo,
    ICurrentUserService currentUser,
    IAuditService audit) : IRequestHandler<UpdateRoomCommand, RoomDto>
{
    public async Task<RoomDto> Handle(UpdateRoomCommand request, CancellationToken ct)
    {
        var tenantId = currentUser.TenantId
            ?? throw new UnauthorizedAccessException("Tenant not found");

        var entity = await repo.GetByIdForUpdateAsync(tenantId, request.Id, ct)
            ?? throw new KeyNotFoundException("Room not found");

        entity.NameEn = request.NameEn.Trim();
        entity.NameAr = request.NameAr.Trim();
        entity.Code = string.IsNullOrWhiteSpace(request.Code) ? null : request.Code.Trim();
        entity.Capacity = request.Capacity;
        entity.IsActive = request.IsActive;

        await repo.SaveChangesAsync(ct);
        await audit.LogAsync("Update", "Room", entity.Id, ct: ct);

        return new RoomDto(entity.Id, entity.NameEn, entity.NameAr, entity.Code, entity.Capacity, entity.IsActive, entity.CreatedAt);
    }
}
