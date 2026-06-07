using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using SchoolCom.Application.Common.Interfaces;
using SchoolCom.Application.Features.Messaging;
using SchoolCom.Domain.Enums;

namespace SchoolCom.Api.Controllers.v1;

[ApiController]
[Route("v{version:apiVersion}/messaging/attachments")]
[Asp.Versioning.ApiVersion("1.0")]
[Authorize]
[EnableRateLimiting("api")]
public class MessageAttachmentsController(
    IMessagingRepository repository,
    IMessagingAccessService access,
    ICurrentUserService currentUser,
    IFileStorage fileStorage) : ControllerBase
{
    [HttpGet("{attachmentId:guid}/download")]
    public async Task<IActionResult> Download(Guid attachmentId, CancellationToken ct)
    {
        var tenantId = access.RequireTenantId();
        var userId = access.RequireUserId();

        var attachment = await repository.GetAttachmentAsync(tenantId, attachmentId, ct);
        if (attachment is null) return NotFound();

        await access.EnsureCanAccessConversationAsync(attachment.Message.ConversationId, ct);

        var isAdmin = MessagingAuthorization.IsAdmin(currentUser.Roles);
        var isSender = attachment.Message.SenderId == userId;
        var isApproved = attachment.Message.ModerationStatus == ModerationStatus.Approved;

        if (!isAdmin && !isSender && !isApproved)
            return Forbid();

        var stream = await fileStorage.OpenReadAsync(attachment.StorageKey, ct);
        if (stream is null) return NotFound();

        return File(stream, attachment.MimeType, attachment.FileName);
    }
}
