using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using SchoolCom.Api.Models;
using SchoolCom.Application.Features.RoomAssignments;
using SchoolCom.Application.Features.RoomAssignments.Commands.CreateRoomAssignment;
using SchoolCom.Application.Features.RoomAssignments.Commands.DeleteRoomAssignment;
using SchoolCom.Application.Features.RoomAssignments.Commands.UpdateRoomAssignment;
using SchoolCom.Application.Features.RoomAssignments.Queries.GetClassSectionOptions;
using SchoolCom.Application.Features.RoomAssignments.Queries.GetRoomAssignments;

namespace SchoolCom.Api.Controllers.v1;

[ApiController]
[Route("v{version:apiVersion}/admin/room-assignments")]
[Asp.Versioning.ApiVersion("1.0")]
[Authorize(Policy = "AdminOnly")]
[EnableRateLimiting("api")]
public class RoomAssignmentsController(IMediator mediator) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetRoomAssignments(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] Guid? schoolClassId = null,
        [FromQuery] Guid? sectionId = null,
        [FromQuery] Guid? roomId = null,
        CancellationToken ct = default)
    {
        var result = await mediator.Send(
            new GetRoomAssignmentsQuery(page, pageSize, schoolClassId, sectionId, roomId), ct);
        return Ok(new ApiResponse<RoomAssignmentListResult>(result));
    }

    [HttpGet("class-section-options")]
    public async Task<IActionResult> GetClassSectionOptions(
        [FromQuery] Guid? schoolClassId = null,
        CancellationToken ct = default)
    {
        var result = await mediator.Send(new GetClassSectionOptionsQuery(schoolClassId), ct);
        return Ok(new ApiResponse<IReadOnlyList<ClassSectionLookupDto>>(result));
    }

    [HttpPost]
    public async Task<IActionResult> CreateRoomAssignment(
        [FromBody] CreateRoomAssignmentRequest request,
        CancellationToken ct)
    {
        var result = await mediator.Send(
            new CreateRoomAssignmentCommand(request.ClassSectionId, request.RoomId), ct);
        return Ok(new ApiResponse<RoomAssignmentDto>(result));
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> UpdateRoomAssignment(
        Guid id,
        [FromBody] UpdateRoomAssignmentRequest request,
        CancellationToken ct)
    {
        var result = await mediator.Send(
            new UpdateRoomAssignmentCommand(id, request.ClassSectionId, request.RoomId), ct);
        return Ok(new ApiResponse<RoomAssignmentDto>(result));
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> DeleteRoomAssignment(Guid id, CancellationToken ct)
    {
        await mediator.Send(new DeleteRoomAssignmentCommand(id), ct);
        return Ok(new ApiResponse<object?>(null));
    }
}
