using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using SchoolCom.Api.Models;
using SchoolCom.Application.Features.Messaging;
using SchoolCom.Application.Features.Messaging.Commands.EnsureConversation;
using SchoolCom.Application.Features.Messaging.Commands.MarkConversationRead;
using SchoolCom.Application.Features.Messaging.Commands.SendMessage;
using SchoolCom.Application.Features.Messaging.Queries;
using SchoolCom.Application.Features.Messaging.Queries.GetConversations;
using SchoolCom.Application.Features.Messaging.Queries.GetLinkedStudents;
using SchoolCom.Application.Features.Messaging.Queries.GetMessages;
using SchoolCom.Domain.Enums;

namespace SchoolCom.Api.Controllers.v1;

[ApiController]
[Route("v{version:apiVersion}/messaging")]
[Asp.Versioning.ApiVersion("1.0")]
[Authorize]
[EnableRateLimiting("api")]
public class MessagingController(IMediator mediator) : ControllerBase
{
    [HttpGet("conversations")]
    public async Task<IActionResult> GetConversations([FromQuery] string? search, CancellationToken ct)
    {
        var result = await mediator.Send(new GetConversationsQuery(search), ct);
        return Ok(new ApiResponse<IReadOnlyList<ConversationListItemDto>>(result));
    }

    [HttpGet("linked-students")]
    public async Task<IActionResult> GetLinkedStudents(CancellationToken ct)
    {
        var result = await mediator.Send(new GetLinkedStudentsQuery(), ct);
        return Ok(new ApiResponse<IReadOnlyList<LinkedStudentDto>>(result));
    }

    [HttpPost("conversations/ensure")]
    public async Task<IActionResult> EnsureConversation(
        [FromBody] EnsureConversationRequest request,
        CancellationToken ct)
    {
        var result = await mediator.Send(new EnsureConversationCommand(request.StudentId), ct);
        return Ok(new ApiResponse<ConversationListItemDto>(result));
    }

    [HttpGet("conversations/{conversationId:guid}/messages")]
    public async Task<IActionResult> GetMessages(
        Guid conversationId,
        [FromQuery] string? cursor,
        [FromQuery] int pageSize = 30,
        [FromQuery] string? search = null,
        CancellationToken ct = default)
    {
        var result = await mediator.Send(
            new GetMessagesQuery(conversationId, cursor, pageSize, search), ct);
        return Ok(new ApiResponse<MessageListResult>(result));
    }

    [HttpPost("conversations/{conversationId:guid}/read")]
    public async Task<IActionResult> MarkRead(Guid conversationId, CancellationToken ct)
    {
        await mediator.Send(new MarkConversationReadCommand(conversationId), ct);
        return Ok(new ApiResponse<object?>(null));
    }

    [HttpPost("conversations/{conversationId:guid}/messages")]
    [RequestSizeLimit(25 * 1024 * 1024)]
    public async Task<IActionResult> SendMessage(
        Guid conversationId,
        CancellationToken ct)
    {
        var content = Request.Form["content"].ToString();
        var typeRaw = Request.Form["messageType"].ToString();
        var messageType = Enum.TryParse<MessageType>(typeRaw, true, out var parsed)
            ? parsed
            : MessageType.Text;

        var attachments = new List<AttachmentInput>();
        foreach (var file in Request.Form.Files)
        {
            var mime = file.ContentType;
            MessagingFileLimits.ValidateMimeType(mime);
            var msgType = MessagingFileLimits.ResolveMessageType(mime);
            MessagingFileLimits.ValidateSize(msgType, file.Length);
            attachments.Add(new AttachmentInput(
                file.FileName,
                mime,
                file.OpenReadStream()));
        }

        if (messageType == MessageType.Text && attachments.Count == 0)
            messageType = attachments.Count > 0
                ? MessagingFileLimits.ResolveMessageType(attachments[0].MimeType)
                : MessageType.Text;

        if (attachments.Count > 0 && messageType == MessageType.Text)
            messageType = MessagingFileLimits.ResolveMessageType(attachments[0].MimeType);

        var result = await mediator.Send(
            new SendMessageCommand(conversationId, content, messageType, attachments), ct);

        return Ok(new ApiResponse<SendMessageResult>(result));
    }

    [HttpGet("notifications")]
    public async Task<IActionResult> GetNotifications(CancellationToken ct)
    {
        var result = await mediator.Send(new GetNotificationsQuery(), ct);
        return Ok(new ApiResponse<IReadOnlyList<NotificationDto>>(result));
    }
}

public record EnsureConversationRequest(Guid StudentId);
