using FluentValidation;
using MediatR;
using SchoolCom.Application.Common.Interfaces;
using SchoolCom.Domain.Entities;

namespace SchoolCom.Application.Features.Rooms.Commands.CreateRoom;

public record CreateRoomCommand(
    string NameEn,
    string NameAr,
    string? Code,
    int? Capacity,
    bool IsActive = true) : IRequest<RoomDto>;

public class CreateRoomCommandValidator : AbstractValidator<CreateRoomCommand>
{
    public CreateRoomCommandValidator()
    {
        RuleFor(x => x.NameEn).NotEmpty().MaximumLength(100);
        RuleFor(x => x.NameAr).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Code).MaximumLength(32);
        RuleFor(x => x.Capacity).GreaterThan(0).When(x => x.Capacity.HasValue);
    }
}

public class CreateRoomCommandHandler(
    IRoomRepository repo,
    ICurrentUserService currentUser,
    IAuditService audit) : IRequestHandler<CreateRoomCommand, RoomDto>
{
    public async Task<RoomDto> Handle(CreateRoomCommand request, CancellationToken ct)
    {
        var tenantId = currentUser.TenantId
            ?? throw new UnauthorizedAccessException("Tenant not found");

        var entity = new Room
        {
            TenantId = tenantId,
            NameEn = request.NameEn.Trim(),
            NameAr = request.NameAr.Trim(),
            Code = string.IsNullOrWhiteSpace(request.Code) ? null : request.Code.Trim(),
            Capacity = request.Capacity,
            IsActive = request.IsActive
        };

        await repo.AddAsync(entity, ct);
        await repo.SaveChangesAsync(ct);
        await audit.LogAsync("Create", "Room", entity.Id, ct: ct);

        return new RoomDto(entity.Id, entity.NameEn, entity.NameAr, entity.Code, entity.Capacity, entity.IsActive, entity.CreatedAt);
    }
}
