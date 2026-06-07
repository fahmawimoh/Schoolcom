using MediatR;
using SchoolCom.Application.Common.Interfaces;

namespace SchoolCom.Application.Features.Rooms.Queries.GetRoomById;

public record GetRoomByIdQuery(Guid Id) : IRequest<RoomDto>;

public class GetRoomByIdQueryHandler(
    IRoomRepository repo,
    ICurrentUserService currentUser) : IRequestHandler<GetRoomByIdQuery, RoomDto>
{
    public async Task<RoomDto> Handle(GetRoomByIdQuery request, CancellationToken ct)
    {
        var tenantId = currentUser.TenantId
            ?? throw new UnauthorizedAccessException("Tenant not found");

        var entity = await repo.GetByIdAsync(tenantId, request.Id, ct)
            ?? throw new KeyNotFoundException("Room not found");

        return new RoomDto(entity.Id, entity.NameEn, entity.NameAr, entity.Code, entity.Capacity, entity.IsActive, entity.CreatedAt);
    }
}
