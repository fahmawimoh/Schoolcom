using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using SchoolCom.Api.Models;
using SchoolCom.Application.Features.Messaging;
using SchoolCom.Application.Features.Messaging.Commands.ModerateMessage;
using SchoolCom.Application.Features.Messaging.Queries;

namespace SchoolCom.Api.Controllers.v1;

[ApiController]
[Route("v{version:apiVersion}/admin/moderation")]
[Asp.Versioning.ApiVersion("1.0")]
[Authorize(Policy = "AdminOnly")]
[EnableRateLimiting("api")]
public class ModerationController(IMediator mediator) : ControllerBase
{
    [HttpGet("pending")]
    public async Task<IActionResult> GetPending(
        [FromQuery] ModerationFilterRequest? filter,
        CancellationToken ct)
    {
        var result = await mediator.Send(new GetPendingModerationQuery(filter?.ToFilter()), ct);
        return Ok(new ApiResponse<IReadOnlyList<PendingModerationDto>>(result));
    }

    [HttpGet("history")]
    public async Task<IActionResult> GetHistory(
        [FromQuery] int limit = 50,
        [FromQuery] ModerationFilterRequest? filter = null,
        CancellationToken ct = default)
    {
        var result = await mediator.Send(new GetModerationHistoryQuery(limit, filter?.ToFilter()), ct);
        return Ok(new ApiResponse<IReadOnlyList<ModerationHistoryDto>>(result));
    }

    [HttpPost("messages/{messageId:guid}/approve")]
    public async Task<IActionResult> Approve(
        Guid messageId,
        [FromBody] ModerationNotesRequest? request,
        CancellationToken ct)
    {
        await mediator.Send(new ApproveMessageCommand(messageId, request?.Notes), ct);
        return Ok(new ApiResponse<object?>(null));
    }

    [HttpPost("messages/{messageId:guid}/reject")]
    public async Task<IActionResult> Reject(
        Guid messageId,
        [FromBody] ModerationNotesRequest? request,
        CancellationToken ct)
    {
        await mediator.Send(new RejectMessageCommand(messageId, request?.Notes), ct);
        return Ok(new ApiResponse<object?>(null));
    }
}

public record ModerationNotesRequest(string? Notes);

public record ModerationFilterRequest(
    Guid? StudentId,
    Guid? ParentUserId,
    Guid? TeacherUserId,
    string? Status,
    DateTime? FromDate,
    DateTime? ToDate)
{
    public ModerationFilter ToFilter() =>
        new(StudentId, ParentUserId, TeacherUserId, Status, FromDate, ToDate);
}
