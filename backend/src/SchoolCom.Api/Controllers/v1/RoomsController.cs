using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using SchoolCom.Api.Models;
using SchoolCom.Application.Features.Rooms;
using SchoolCom.Application.Features.Rooms.Commands.CreateRoom;
using SchoolCom.Application.Features.Rooms.Commands.DeleteRoom;
using SchoolCom.Application.Features.Rooms.Commands.UpdateRoom;
using SchoolCom.Application.Features.Rooms.Queries.GetRoomById;
using SchoolCom.Application.Features.Rooms.Queries.GetRooms;

namespace SchoolCom.Api.Controllers.v1;

[ApiController]
[Route("v{version:apiVersion}/admin/rooms")]
[Asp.Versioning.ApiVersion("1.0")]
[Authorize(Policy = "AdminOnly")]
[EnableRateLimiting("api")]
public class RoomsController(IMediator mediator) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetRooms(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string? search = null,
        [FromQuery] bool? isActive = null,
        [FromQuery] string? sortBy = null,
        [FromQuery] bool sortDesc = false,
        CancellationToken ct = default)
    {
        var result = await mediator.Send(
            new GetRoomsQuery(page, pageSize, search, isActive, sortBy, sortDesc), ct);
        return Ok(new ApiResponse<RoomListResult>(result));
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetRoom(Guid id, CancellationToken ct)
    {
        var result = await mediator.Send(new GetRoomByIdQuery(id), ct);
        return Ok(new ApiResponse<RoomDto>(result));
    }

    [HttpPost]
    public async Task<IActionResult> CreateRoom(
        [FromBody] CreateRoomRequest request,
        CancellationToken ct)
    {
        var result = await mediator.Send(
            new CreateRoomCommand(request.NameEn, request.NameAr, request.Code, request.Capacity, request.IsActive), ct);
        return CreatedAtAction(nameof(GetRoom), new { id = result.Id }, new ApiResponse<RoomDto>(result));
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> UpdateRoom(
        Guid id,
        [FromBody] UpdateRoomRequest request,
        CancellationToken ct)
    {
        var result = await mediator.Send(
            new UpdateRoomCommand(id, request.NameEn, request.NameAr, request.Code, request.Capacity, request.IsActive), ct);
        return Ok(new ApiResponse<RoomDto>(result));
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> DeleteRoom(Guid id, CancellationToken ct)
    {
        await mediator.Send(new DeleteRoomCommand(id), ct);
        return Ok(new ApiResponse<object?>(null));
    }
}
