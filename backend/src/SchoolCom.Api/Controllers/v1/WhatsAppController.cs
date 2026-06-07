using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using SchoolCom.Api.Models;
using SchoolCom.Application.Common.Interfaces;
using SchoolCom.Application.Features.WhatsApp.Commands.SendWhatsAppMessage;

namespace SchoolCom.Api.Controllers.v1;

[ApiController]
[Route("v{version:apiVersion}/admin/whatsapp")]
[Asp.Versioning.ApiVersion("1.0")]
[Authorize(Policy = "AdminOnly")]
[EnableRateLimiting("api")]
public class WhatsAppController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly IWhatsAppService _whatsApp;

    public WhatsAppController(IMediator mediator, IWhatsAppService whatsApp)
    {
        _mediator = mediator;
        _whatsApp = whatsApp;
    }

    [HttpGet("config")]
    public async Task<IActionResult> GetConfig(CancellationToken ct)
    {
        var tenantId = User.FindFirst("tenant_id")?.Value;
        if (!Guid.TryParse(tenantId, out var id))
            return Unauthorized();

        var (connected, phoneNumberId) = await _whatsApp.GetConfigAsync(id, ct);
        return Ok(new ApiResponse<object>(new { connected, phoneNumberId }));
    }

    [HttpPost("send")]
    public async Task<IActionResult> Send([FromBody] WhatsAppSendRequest request, CancellationToken ct)
    {
        var result = await _mediator.Send(
            new SendWhatsAppMessageCommand(request.Phone, request.Message), ct);
        return Ok(new ApiResponse<object>(new { messageId = result.MessageId }));
    }

    [HttpGet("conversations")]
    public async Task<IActionResult> GetConversations(CancellationToken ct)
    {
        var tenantId = User.FindFirst("tenant_id")?.Value;
        if (!Guid.TryParse(tenantId, out var id))
            return Unauthorized();

        var conversations = await _whatsApp.GetConversationsAsync(id, ct);
        return Ok(new ApiResponse<object>(conversations));
    }

    [HttpGet("webhook")]
    [AllowAnonymous]
    public IActionResult VerifyWebhook(
        [FromQuery(Name = "hub.mode")] string mode,
        [FromQuery(Name = "hub.verify_token")] string token,
        [FromQuery(Name = "hub.challenge")] string challenge)
    {
        var verifyToken = HttpContext.RequestServices
            .GetRequiredService<IConfiguration>()["WhatsApp:WebhookVerifyToken"];

        if (mode == "subscribe" && token == verifyToken)
            return Content(challenge);

        return Forbid();
    }

    [HttpPost("webhook")]
    [AllowAnonymous]
    public IActionResult ReceiveWebhook([FromBody] object payload)
    {
        // Process inbound WhatsApp messages from Meta webhook
        return Ok();
    }
}

public record WhatsAppSendRequest(string Phone, string Message);
